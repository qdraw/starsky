; ----------------------------------------------------------------------------------------
; This is a Win64 console program that writes "Hello" on one line and then exits.  It
; uses puts from the C library.  To assemble and run:
; https://cs.lmu.edu/~ray/notes/nasmtutorial/
;     nasm -fwin64 hello.asm && gcc hello.obj && a
; ----------------------------------------------------------------------------------------

        global  main
        extern  puts
        section .text
main:
        sub     rsp, 28h                        ; Reserve the shadow space
        mov     rcx, message                    ; First argument is address of message
        call    puts                            ; puts(message)
        add     rsp, 28h                        ; Remove shadow space
        ret
message:
        db      'Fake ExifTool', 0                      ; C strings need a zero byte at the end
