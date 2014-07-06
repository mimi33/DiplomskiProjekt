import os

imeDatoteke = "PodaciZaUsporedbu/drugoPolugodiste"
imeDirektorija = imeDatoteke+"-Eval"

podijeliPodatke = True

if not os.path.exists(imeDirektorija):
	os.makedirs(imeDirektorija)

filePotrosnja = open(imeDatoteke + ".csv", "r").readlines()
prviRed = filePotrosnja.pop(0)
fajloviPoMjesecima = [open(imeDirektorija + "/mjesec"+str(i+1)+".csv", "w") for i in range(12)]

for i in range(12):
	fajloviPoMjesecima[i].write(prviRed)

prviMjesec = True
prijasnjih6Dana = []
trenutniMjesec = None
for line in filePotrosnja:
	mjesec = int(line.split(';')[2])
	print mjesec
	if trenutniMjesec is None:
		trenutniMjesec = mjesec

	if trenutniMjesec != mjesec:
		#poceo je drugi mjesec
		#zapisati sacuvane podatke
		for savedLine in prijasnjih6Dana:
			fajloviPoMjesecima[mjesec - 1].write(savedLine)
		#zapisati nove podatke i isprazniti varijable
		fajloviPoMjesecima[mjesec - 1].write(line)
		prijasnjih6Dana = [line]
		trenutniMjesec = mjesec
		prviMjesec = False
	else: # trenutniMjesec == mjesec
		#ako smo negdje u sred mjeseca
		if not prviMjesec:
			fajloviPoMjesecima[mjesec - 1].write(line)
		prijasnjih6Dana.append(line)
		#ako smo sacuvali previse podataka
		if len(prijasnjih6Dana) > 144:
			prijasnjih6Dana.pop(0)

for fajl in fajloviPoMjesecima:
	fajl.close()
