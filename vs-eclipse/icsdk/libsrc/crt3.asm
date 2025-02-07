; linkage glue for IC3 - cartridge format
; do not change unless you *REALLY* know what you are doing

$maximum

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Data
f_area section data large

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Code (in cartridge)

f_cart_bank0 section code large

    ;; special Cartridge detect (don't erase E2PROM)
    DW 0xA05F ; must be this value

    ;; first part of ROM can be used as a "printf" area
    DB "I-Cybie Cartridge ROM", 13, 10
    DB "from ICSDK2 - ALPHA", 13, 10
    DB "(c) 2002 ICybiePet", 13, 10
    DB "http://aibohack.com/icybie", 13, 10

;;;;;;;;;;;;;;;;;;;;;;
extern ._ic_startup


f_cart_start section code large ; middle of ROM

;; 20FFFE
    DW 0xFFFF    ;; if 0xABCD - middle part of E2PROM gets erased
                ;; which breaks standard ICybie personality
;; 210000
    public _cartridge_entry
_cartridge_entry:
    EI 7    ; disable interrupts
    JP  ._ic_startup


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    END
