a = 0
b = 0
label2:
	a = a + 2 : b = b + a + 2 
	print a, b
	if a < 10 then print ":) " : goto label2
	'for i=0 to n
	'	print i
	'next i
print "end " + a + " " + b
