 IFNDEF NEWFEAT
    fatal "bad include"
 ENDIF

;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;
; new RAM variables

boolSending EQU 1700H
byteWagTailUpperCount EQU 1701H
boolStartupPerformed EQU 1702H
boolNoSubstitute EQU 1703H
wSslSkipCounter EQU 1704H

;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;==;;
;; Constants

 IFDEF SERIAL
PRINTF_A_DECIMAL EQU 80H
PRINTF_W_DECIMAL EQU 81H
PRINTF_B_DECIMAL EQU 82H
PRINTF_C_DECIMAL EQU 83H
PRINTF_XWA_HEX EQU 84H
 ENDIF


