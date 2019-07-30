If 1 = 1 Then
assert 1
Else
assert 0
EndIf

If 1 = 1 Then
If 2 = 3 Then
assert 0
EndIf
Else
assert 0
EndIf

If 1 = 1 Then
If 2 = 3 Then
assert 0
Else
assert 1
EndIf
Else
assert 0
EndIf
