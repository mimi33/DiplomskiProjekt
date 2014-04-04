
filePotrosnja = open("potrosnja.csv", "r").readlines()
filePotrosnja.pop(0)
fajloviPoSatovima = [open("./PoSatima/sat"+str(i)+".txt", "w") for i in range(24)]

for line in filePotrosnja:
	line = line.split(';')
	sat = int(line.pop(0))
	fajloviPoSatovima[sat].write(';'.join(line))

for fajl in fajloviPoSatovima:
	fajl.close()