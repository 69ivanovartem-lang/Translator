data segment
x  dw    1
y  dw    1
number  dw    1
sum  dw    1
i  dw    1
remainder  dw    1
test  dw    1
PRINT_BUF DB ' ' DUP(10)
BUFEND    DB '$'
data ends
stk segment stack
db 256 dup ("?")
stk ends
code segment
assume cs:code,ds:data,ss:stk
start:
main proc
mov ax,data
mov ds,ax
mov ax, 12365
push ax
pop ax
mov number, ax
mov ax, number
push ax
mov ax, 1
push ax
pop bx
pop ax
add ax, bx
push ax
mov ax, 2
push ax
pop bx
pop ax
mul bx
push ax
mov ax, number
push ax
mov ax, 3
push ax
pop bx
pop ax
sub ax, bx
push ax
pop bx
pop ax
cwd
div bl
push ax
pop ax
mov test, ax
mov ax, test
push ax
CALL PRINT
pop ax
mov ax,4c00h
int 21h
main endp
PRINT PROC NEAR
MOV CX, 10
MOV DI, BUFEND - PRINT_BUF
PRINT_LOOP:
MOV DX, 0
DIV CX
ADD DL, '0'
MOV [PRINT_BUF + DI - 1], DL
DEC DI
CMP AL, 0
JNE PRINT_LOOP
LEA DX, PRINT_BUF
ADD DX, DI
MOV AH, 09H
INT 21H
RET
PRINT ENDP
code ends
end main