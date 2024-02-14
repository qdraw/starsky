#ifndef MACOS_STUB_H
#define MACOS_STUB_H

#include <Windows.h>

#ifdef __cplusplus
extern "C" {
#endif

    __declspec(dllexport) void* CFStringCreateWithBytes(void* allocator, void* buffer, long bufferLength, void* encoding, bool isExternalRepresentation);
    __declspec(dllexport) void* CreateCfString(const char* aString);
    __declspec(dllexport) void* CreateCfArray(void** objects, long numObjects);
    __declspec(dllexport) void CFRelease(void* handle);
    __declspec(dllexport) void* objc_getClass(const char* name);
    __declspec(dllexport) void* NSSelectorFromString(void* cfstr);
    __declspec(dllexport) void* objc_msgSend_retIntPtr(void* target, void* selector);
    __declspec(dllexport) void objc_msgSend_retVoid_IntPtr_IntPtr(void* target, void* selector, void* param1, void* param2);

#ifdef __cplusplus
}
#endif

#endif // MACOS_STUB_H
