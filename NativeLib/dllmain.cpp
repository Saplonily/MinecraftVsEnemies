// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.hpp"

#define native_export __declspec(dllexport)

extern "C"
{
	native_export HRESULT __stdcall ComInit()
	{
		return CoInitialize(NULL);
	}

	native_export LPWSTR __stdcall OpenDialog(const COMDLG_FILTERSPEC* fileTypes, int size)
	{
		IFileDialog* pfd = NULL;
		HRESULT hr = CoCreateInstance(CLSID_FileOpenDialog, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pfd));
		DWORD dwFlags;
		hr = pfd->GetOptions(&dwFlags);
		hr = pfd->SetOptions(dwFlags | FOS_FORCEFILESYSTEM);
		hr = pfd->SetFileTypes(size, fileTypes);
		hr = pfd->SetFileTypeIndex(1);
		hr = pfd->Show(NULL);
		if (hr == HRESULT_FROM_WIN32(ERROR_CANCELLED))
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

	native_export void __stdcall ComFree(const void* ptr)
	{
		CoTaskMemFree((LPVOID)ptr);
	}
}