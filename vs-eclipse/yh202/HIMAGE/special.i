;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; special cases to work around assembler

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

; Move a byte from memory to memory
;  for some reason the standard LDB opcode does not work
;  LDBB var_d,var_s == LDB (var_d),(var_s)

LDBB MACRO vdest,vsrc
    DB 0C1H
    DW vsrc
    DB 19H
    DW vdest
 ENDM

LDWW MACRO vdest,vsrc
    DB 0D1H
    DW vsrc
    DB 19H
    DW vdest
 ENDM

LDB_XIXPLUS MACRO vdest
    DB 0C5H
    DW vdest
    DB 5AH, 12H
 ENDM

LDB_XIX MACRO vdest
    DB 94H, 19H
    DW vdest
 ENDM

LDIB_X MACRO
    DB 85H, 10H
 ENDM

LDDRB_X MACRO
    DB 85H, 13H
 ENDM

POPL MACRO reg ; bug in real one
	IF "reg" == "XWA"
        DB 58H
    ENDIF
    IF "reg" == "XBC"
        DB 59H
    ENDIF
    IF "reg" == "XDE"
        DB 5AH
    ENDIF
    IF "reg" == "XHL"
        DB 5BH
    ENDIF
    IF "reg" == "XIX"
        DB 5CH
    ENDIF
    IF "reg" == "XIY"
        DB 5DH
    ENDIF
    IF "reg" == "XIZ"
        DB 5EH
    ENDIF
    IF "reg" == "XSP"
        DB 5FH
    ENDIF
 ENDM


POPW MACRO reg ; bug in real one
	IF "reg" == "WA"
        DB 48H
    ENDIF
    IF "reg" == "BC"
        DB 49H
    ENDIF
    IF "reg" == "DE"
        DB 4AH
    ENDIF
    IF "reg" == "HL"
        DB 4BH
    ENDIF
    IF "reg" == "IX"
        DB 4CH
    ENDIF
    IF "reg" == "IY"
        DB 4DH
    ENDIF
    IF "reg" == "IZ"
        DB 4EH
    ENDIF
    IF "reg" == "SP"
        DB 4FH
    ENDIF
 ENDM

; NOP - if exact binary version needed
NOP_EXACT MACRO
    IFDEF EXACT
	   NOP
    ENDIF
 ENDM

ORG_EXACT MACRO addr
    IFDEF EXACT
	    IF $ <> addr
	        FATAL "ORG_EXACT misalign"
        ENDIF
    ENDIF
    ORG addr
 ENDM


LDL_ACTION MACRO reg1,defact,index
    IFDEF EXACT
        LDL reg1,defact
    ELSE
        LDL reg1,(INT(EXTRA_ACTION_TABLE+index*4))
DEFXA_index EQU defact
    ENDIF
 ENDM

SEND_EYES MACRO  val
			LDB A,val
$$retry:
			CALL SendAudioByteToDaughter
			JR NC,$$retry
 ENDM

WAIT_1SEC MACRO ;; actually 1->1.5 sec
			LDB (countdown590_2Hz),02H
$$wait:
			CPB (countdown590_2Hz),0
			JR NZ,$$wait
 ENDM
 
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
