
filePotrosnja = open("ISO-NE.csv", "r")
filePotrosnja.readline() #header
prviRed = "Sat;RadniDan;Mjesec;PrijasnjaPotrosnja;Potrosnja\n"

FajloviZaProvjeru = [open("Eval/mjesec"+str(i+1)+".csv", "w") for i in range(12)]
for fajl in FajloviZaProvjeru:
	fajl.write(prviRed)

FajloviZaUcenje = [open("PoSatima/sat"+str(i)+".txt", "w") for i in range(24)]
for fajl in FajloviZaUcenje:
	fajl.write(prviRed.split(";",1)[1])

prijasnjaPotrosnja = "NaN"
danUTjednu = 7 # 26.12.2004 je nedjelja
#raspon datuma je od 2004-2006, ukljucivo
praznici = [[24,12,2004],
			[31,12,2004], [17, 1,2005], [21, 2,2005], [30, 5,2005], [ 4, 7,2005],
			[ 5, 9,2005], [10,10,2005], [11,11,2005], [24,11,2005], [26,12,2005],
			[ 2, 1,2006], [16, 1,2006], [20, 2,2006], [29, 5,2006], [ 4, 7,2006],
			[ 4, 9,2006], [ 9,10,2006], [10,11,2006], [23,11,2006], [25,12,2006]]

for line in filePotrosnja:
	podaci = line.rstrip().split(";")
	(dd,mm,gg) = [int(x) for x in podaci[0].split(".")]
	sat = int(podaci[1]) - 1  #sat je 1-24, a treba 0-23
	temp = 5 / 9 * (float(podaci[2]) - 32)   #temp je u F, a treba C
	potrosnja = podaci[3]
	radniDan = 1
	if [dd,mm,gg] in praznici or danUTjednu >= 6:
		radniDan = 0

	if gg < 2006:
		#podaci za ucenje - raskomadati
		FajloviZaUcenje[sat].write(";".join(str(x) for x in [radniDan, mm, prijasnjaPotrosnja, potrosnja]) + "\n")
		# dodati 6 dana na pocetak skupa za provjeru da se ne izgubi 1. mjesec
		if gg == 2005 and mm == 12 and dd > 25:
			FajloviZaProvjeru[0].write(";".join(str(x) for x in [sat, radniDan, mm, prijasnjaPotrosnja, potrosnja]) + "\n")
	else:
		#podaci za provjeru - podijeliti po mjesecima
		# dodati u prijasnju datoteku da se ne izgubi prvih 6 dana
		if dd > 25:
			FajloviZaProvjeru[mm - 2].write(";".join(str(x) for x in [sat, radniDan, mm, prijasnjaPotrosnja, potrosnja]) + "\n")
		FajloviZaProvjeru[mm - 1].write(";".join(str(x) for x in [sat, radniDan, mm, prijasnjaPotrosnja, potrosnja]) + "\n")

	if sat == 23:
		danUTjednu = (danUTjednu % 7) + 1
	prijasnjaPotrosnja = potrosnja

filePotrosnja.close()
for fajl in FajloviZaUcenje:
	fajl.close()
for fajl in FajloviZaProvjeru:
	fajl.close()