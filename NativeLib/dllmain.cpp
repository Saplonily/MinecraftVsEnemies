// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.hpp"

#define native_export __declspec(dllexport) __stdcall

extern "C"
{
	HRESULT native_export ComInit()
	{
		return CoInitialize(NULL);
	}

	LPWSTR native_export OpenDialog(const COMDLG_FILTERSPEC* fileTypes, int size)
	{
		IFileDialog* pfd = NULL;
		HRESULT hr = CoCreateInstance(CLSID_FileOpenDialog, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pfd));
		DWORD dwFlags;
		hr = pfd->GetOptions(&dwFlags);
		hr = pfd->SetOptions(dwFlags | FOS_FORCEFILESYSTEM);
		hr = pfd->SetFileTypes(size, fileTypes);
		hr = pfd->SetFileTypeIndex(1);
		hr = pfd->Show(NULL);
		if (!SUCCEEDED(hr))
		{
			pfd->Release();
			return nullptr;
		}
		IShellItem* pSelItem;
		hr = pfd->GetResult(&pSelItem);
		LPWSTR* pszFilePath = new LPWSTR;
		hr = pSelItem->GetDisplayName(SIGDN_DESKTOPABSOLUTEPARSING, pszFilePath);
		pSelItem->Release();
		pfd->Release();
		return *pszFilePath;
	}

	void native_export ComFree(const void* ptr)
	{
		CoTaskMemFree((LPVOID)ptr);
	}

	int native_export NativeMessageBox(const wchar_t* title, const wchar_t* content)
	{
		return MessageBoxW(NULL, content, title, MB_OK);
	}
}