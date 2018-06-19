#include "TextOverlay.h"

static inline HRESULT GetPixleFormat(int bpp, PixelFormat* pixFmt)
{
	switch(bpp)
	{
		case 15:
			*pixFmt = PixelFormat16bppRGB555;
			return S_OK;

		case 16:
			*pixFmt = PixelFormat16bppRGB565;
			return S_OK;

		case 24:
			*pixFmt = PixelFormat24bppRGB;
			return S_OK;

		case 32:
			*pixFmt = PixelFormat32bppRGB;
			return S_OK;

		default:
			return E_FAIL;
	}
}

CTextOverlay::CTextOverlay(LPUNKNOWN pUnk, HRESULT *phr)
	: CTransInPlaceFilter(FILTER_NAME, pUnk, CLSID_TextOverlay, phr)
{
	GdiplusStartupInput gdiplusStartupInput; 
    GdiplusStartup(&m_gdiplusToken, &gdiplusStartupInput, NULL);
}

CTextOverlay::~CTextOverlay(void)
{
	GdiplusShutdown(m_gdiplusToken);
}

HRESULT CTextOverlay::CheckInputType(const CMediaType* mtIn)
{
	CAutoLock lock(m_pLock);

    if (mtIn->majortype != MEDIATYPE_Video)
    {
        return E_FAIL;
    }

	if(mtIn->subtype != MEDIASUBTYPE_RGB555 &&
	   mtIn->subtype != MEDIASUBTYPE_RGB565 &&
	   mtIn->subtype != MEDIASUBTYPE_RGB24 &&
	   mtIn->subtype != MEDIASUBTYPE_RGB32)
	{
		return E_FAIL;
	}

    if ((mtIn->formattype == FORMAT_VideoInfo) &&
        (mtIn->cbFormat >= sizeof(VIDEOINFOHEADER)) &&
        (mtIn->pbFormat != NULL))
    {
        return S_OK;
    }
        
    return E_FAIL;
}

HRESULT CTextOverlay::SetMediaType(PIN_DIRECTION direction, const CMediaType *pmt)
{
	if(direction == PINDIR_INPUT)
	{
		VIDEOINFOHEADER* pvih = (VIDEOINFOHEADER*)pmt->pbFormat;
		m_videoInfo = *pvih;
		HRESULT hr = GetPixleFormat(m_videoInfo.bmiHeader.biBitCount, &m_pixFmt);
		if(FAILED(hr))
		{
			return hr;
		}

		BITMAPINFOHEADER bih = m_videoInfo.bmiHeader;
		m_stride = bih.biBitCount / 8 * bih.biWidth;
	}

	return S_OK;
}
	 
HRESULT CTextOverlay::Transform(IMediaSample *pSample)
{
	CAutoLock lock(m_pLock);

	BYTE* pBuffer = NULL;
	Status s = Ok;
	map<DWORD, Overlay*>::iterator it;

	HRESULT hr = pSample->GetPointer(&pBuffer);
	if(FAILED(hr))
	{
		return hr;
	}

	BITMAPINFOHEADER bih = m_videoInfo.bmiHeader;
	Bitmap bmp(bih.biWidth, bih.biHeight, m_stride, m_pixFmt, pBuffer);
	Graphics g(&bmp);

	for ( it = m_overlays.begin() ; it != m_overlays.end(); it++ )
	{
		Overlay* over = (*it).second;
		if (over->visible == 0)
			continue;
		
		if (over->type == 0)
		{
			SolidBrush brush(over->color);
			Font font(FontFamily::GenericSerif(), over->fontSize);
			s = g.DrawString(over->text, -1, &font, over->pos, StringFormat::GenericDefault(), &brush);
			if (s != Ok)
			{
				TCHAR msg[100];
				wsprintf(L"Failed to draw text : %s", over->text);
				::OutputDebugString(msg);
			}
		}

		if (over->type == 1)
		{
		
			Pen MyPen(over->color, over->lineWidth);  // A green pen, with full alpha
			g.DrawLine(&MyPen, over->x1, over->y1, over->x2, over->y2);
		}

	}

	return S_OK;
}

CUnknown *WINAPI CTextOverlay::CreateInstance(LPUNKNOWN pUnk, HRESULT *phr)
{
	CTextOverlay *overlay = new CTextOverlay(pUnk, phr);
	if (!overlay) 
	{
		if (phr) 
			*phr = E_OUTOFMEMORY;
	}

	return overlay;
}

STDMETHODIMP CTextOverlay::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
	CheckPointer(ppv, E_POINTER);

	if(riid == IID_ITextAdditor) 
	{
		return GetInterface((ITextAdditor*) this, ppv);
	} 
	
	return CTransInPlaceFilter::NonDelegatingQueryInterface(riid, ppv);
}


HRESULT CTextOverlay::AddLine(DWORD id,
							  LONG   x1,
							  LONG   y1,
							  LONG   x2,
							  LONG   y2,
							  COLORREF color,
							  int width)

{

	CAutoLock lock(m_pLock);

	map<DWORD, Overlay*>::iterator it;
	if ((it = m_overlays.find(id)) != m_overlays.end()) // id already in the map
	{	
		Overlay* over = (*it).second;		
		over->type = 1;
		over->visible = 1;
		over->color.SetFromCOLORREF(color);
		over->fontSize = 0;

		over->x1 = x1;
		over->x2 = x2;
		over->y1 = y1;
		over->y2 = y2;
		over->lineWidth = width;

		m_overlays[id] = over;

		return S_OK;
	}
	else {

		Overlay* overlay = new Overlay();
		overlay->type = 1;
		overlay->visible = 1;
		overlay->color.SetFromCOLORREF(color);
		overlay->fontSize = 0;

		overlay->x1 = x1;
		overlay->x2 = x2;
		overlay->y1 = y1;
		overlay->y2 = y2;
		overlay->lineWidth = width;

		m_overlays[id] = overlay;

		return S_OK;
	}

}

HRESULT CTextOverlay::AddTextOverlay(WCHAR* text, DWORD id, 
									LONG    left,
									LONG    top,
									LONG    right,
									LONG    bottom, 
									COLORREF color, float fontSize)
{
	CAutoLock lock(m_pLock);

	if(m_overlays.find(id) != m_overlays.end()) // id already in the map
	{
		return E_INVALIDARG;
	}

	Overlay* overlay = new Overlay(text);
	overlay->type = 0;
	overlay->visible = 1;
	overlay->color.SetFromCOLORREF(color);
	overlay->fontSize = fontSize;
	RectF rcBounds(left, top, right, bottom);
	overlay->pos = rcBounds;

	m_overlays[id] = overlay;

	return S_OK;
}

HRESULT CTextOverlay::Clear(void)
{
	CAutoLock lock(m_pLock);

	map<DWORD, Overlay*>::iterator it;
	for ( it = m_overlays.begin() ; it != m_overlays.end(); it++ )
	{
		Overlay* over = (*it).second;
		over->visible = 0;
	}

	return S_OK;
}

HRESULT CTextOverlay::Remove(DWORD id)
{
	CAutoLock lock(m_pLock);


 

	if (m_overlays[id] != NULL)
	{
		m_overlays[id]->visible = 0;
		//delete m_overlays[id];
		//m_overlays.erase(id);
	}

	return S_OK;
}
