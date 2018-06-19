#pragma once

#include "streams.h"
#include "gdiplus.h"
#include <wchar.h>
#include <map>
#include "Overlay.h"
#include "ITextAdditor.h"

using namespace Gdiplus;
using namespace std;

class CTextOverlay : public CTransInPlaceFilter, public ITextAdditor
{
public:
	DECLARE_IUNKNOWN;

	CTextOverlay(LPUNKNOWN pUnk, HRESULT *phr);
	virtual ~CTextOverlay(void);

	virtual HRESULT CheckInputType(const CMediaType* mtIn);
	virtual HRESULT SetMediaType(PIN_DIRECTION direction, const CMediaType *pmt);
	virtual HRESULT Transform(IMediaSample *pSample);

	static CUnknown *WINAPI CreateInstance(LPUNKNOWN pUnk, HRESULT *phr); 
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

	STDMETHODIMP AddTextOverlay(WCHAR* text, DWORD id, LONG    left,
		LONG    top,
		LONG    right,
		LONG    bottom, COLORREF color = RGB(255, 255, 255), float fontSize = 20);
	STDMETHODIMP Clear(void);	
	STDMETHODIMP Remove(DWORD id);
	STDMETHODIMP AddLine(DWORD id, LONG  x1, LONG   y1, LONG    x2, LONG    y2, COLORREF color, int width);


private:
	ULONG_PTR m_gdiplusToken;
	VIDEOINFOHEADER m_videoInfo;
	PixelFormat m_pixFmt;
	int m_stride;
	map<DWORD, Overlay*> m_overlays;
};

