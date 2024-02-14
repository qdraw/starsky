Visual studio installer -> Modify
-> Desktop Enviorment with C++


To compile the C++ code using the command line on Windows, you can use the Visual Studio Command Prompt, which sets up the necessary environment variables and paths for using the Visual C++ compiler (cl.exe) and other tools. Here's how you can compile the code:

    Open the Visual Studio Command Prompt:
        Press the Windows key and type "Visual Studio Command Prompt".
        Open the command prompt with the appropriate version of Visual Studio you're using (e.g., Developer Command Prompt for Visual Studio 2019).

    Navigate to the directory containing your C++ source files:
        Use the cd command to change to the directory where MacOsStub.cpp and MacOsStub.h are located.

    Compile the code:
        Use the cl command to compile the C++ code and generate the DLL. Here's a basic command:

    cl /EHsc /LD /arch=arm64 MacOsStub.cpp

        /EHsc enables standard C++ exception handling.
        /LD specifies that the output should be a DLL.
        MacOsStub.cpp is the name of your C++ source file.

Verify the output:

    After a successful compilation, you should find the compiled DLL in the same directory as your source files.