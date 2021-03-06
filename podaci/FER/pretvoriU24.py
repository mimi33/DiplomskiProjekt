import os

imeDatoteke = "PodaciZaUsporedbu/drugoPolugodiste"

podijeliPodatke = True

if not os.path.exists(imeDatoteke):
	os.makedirs(imeDatoteke)

filePotrosnja = open(imeDatoteke + ".csv", "r").readlines()
prviRed = filePotrosnja.pop(0)
prviRed = prviRed.split(";",1)[1]
fajloviPoSatovima = [open( imeDatoteke + "/sat"+str(i)+".txt", "w") for i in range(24)]

for i in range(24):
	fajloviPoSatovima[i].write(prviRed)


for line in filePotrosnja:
	line = line.split(';')
	sat = int(line.pop(0))
	fajloviPoSatovima[sat].write(';'.join(line))

for fajl in fajloviPoSatovima:
	fajl.close()
