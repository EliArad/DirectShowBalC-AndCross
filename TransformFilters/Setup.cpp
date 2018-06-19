#include "TextOverlay.h"

// Media Types
const AMOVIESETUP_MEDIATYPE sudPinTypes[] =   
{ 
	{ 
		&MEDIATYPE_Video, 
	   &MEDIASUBTYPE_NULL 
	}
};

// Pins
const AMOVIESETUP_PIN psudPins[] = 
{ 
	{ 
		L"Input", 
			FALSE,
			FALSE, 
			FALSE, 
			FALSE, 
			&CLSID_NULL, 
			NULL,
			1, 
			&sudPinTypes[0] 
	}, 
	{ 
		L"Output", 
			FALSE, 
			TRUE, 
			FALSE, 
			FALSE, 
			&CLSID_NULL,
			NULL, 
			1, 
			&sudPinTypes[0]
	} 
};   

// Filters
const AMOVIESETUP_FILTER sudAudioVolume = 
{ 
	&CLSID_TextOverlay, 
	FILTER_NAME, 
	MERIT_UNLIKELY,
	2, 
	psudPins
};                     

// Templates
CFactoryTemplate g_Templates[]=
{
	{ 
		FILTER_NAME, 
		&CLSID_TextOverlay,
		CTextOverlay::CreateInstance, 
		NULL, 
		&sudAudioVolume 
	}
};

int g_cTemplates = sizeof(g_Templates)/sizeof(g_Templates[0]);

STDAPI DllRegisterServer() 
{
	return AMovieDllRegisterServer2(TRUE);
}

STDAPI DllUnregisterServer()
{
	return AMovieDllRegisterServer2(FALSE);
}
