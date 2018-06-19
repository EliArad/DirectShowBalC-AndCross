#pragma once

#define FILTER_NAME L"Shapes Overlay Filter"

// {E52BEAB4-45FB-4D5A-BC9E-2381E61DCC47}
static const GUID CLSID_TextOverlay = 
{ 0xe52beab4, 0x45fb, 0x4d5a, { 0xbc, 0x9e, 0x23, 0x81, 0xe6, 0x1d, 0xcc, 0x47 } };

// {B6F36855-D861-4ADB-B76F-5F3CF52410AC}
static const GUID IID_ITextAdditor = 
{ 0xb6f36855, 0xd861, 0x4adb, { 0xb7, 0x6f, 0x5f, 0x3c, 0xf5, 0x24, 0x10, 0xac } };

DECLARE_INTERFACE_(ITextAdditor, IUnknown)
{
	STDMETHOD(AddTextOverlay)(WCHAR* text, DWORD id, 
							 LONG    left,
							 LONG    top,
							 LONG    right,
							 LONG    bottom	, 
							 COLORREF color, 
							 float fontSize) PURE;
	STDMETHOD(Clear)(void) PURE;
	STDMETHOD(Remove)(DWORD id) PURE;
	STDMETHOD(AddLine)(DWORD id, LONG    x1, LONG   y1, LONG    x2, LONG    y2, COLORREF color, int width) PURE;
};