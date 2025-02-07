; TLCS900 C Compiler Runtime Routine
;
;		Copyright(C) 1994 TOSHIBA CORPORATION ALL RIGHT RESERVED
;
; XWA * XBC = 
;    (QWA*0x10000+WA)(QBC*0x10000+BC)
;    =  QWA*QBC*0x100000000 +
;       QWA*BC*0x10000 + QBC*WA*0x10000 +
;		WA*BC
;    =  (QWA*BC + QBC*WA)*0x10000 + 
;		WA*BC

$maximum
f_code		section code	large
		public	C9H_mullu
		public	C9H_mulls
C9H_mullu:
C9H_mulls:
		LD		HL,QWA
		MUL		XHL,BC
		LD		DE,QBC
		MUL		XDE,WA
		ADD		XHL,XDE
		LD		QHL,HL	;	SLL 16,XHL
		LD		HL,0
		MUL		XWA,BC
		ADD		XHL,XWA
		RET
		end
