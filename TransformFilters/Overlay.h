#pragma once

#include "gdiplus.h"
#include <wchar.h>

using namespace Gdiplus;

struct Overlay
{
	int type;
	int visible;
	WCHAR* text;
	RectF pos;
	Color color;
	float fontSize;
	int x1;
	int y1;
	int x2;
	int y2;
	int lineWidth;

	Overlay(WCHAR* overlayText)
	{
		int size = wcslen(overlayText);
		text = new WCHAR[size];
		wmemcpy(this->text, overlayText, size);
		this->text[size] = '\0';
	}

	Overlay()
	{

	}

	~Overlay()
	{
		delete text;
	}
};