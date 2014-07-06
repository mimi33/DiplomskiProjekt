

gg = 2011
podaci = dict() # key je hhddmmyyyy, nema vodecih nula nego su praznine
fileKonacni = open("PodaciZaUsporedbu/sviPodaci.csv", "w")
fileKonacni.write("Sat;Praznik;Mjesec;PrijasnjaPotrosnja;Potrosnja\n")
fileVrijeme = open("ZGM_satni.txt", "r").readlines()
fileVrijeme.pop(0)
fileVrijeme.pop(0)
fileVrijeme.pop(0)
for line in fileVrijeme:
	if not line.rstrip('\n'): continue
	podaci[line[0:10]] = [float(line[22:26]) / 10,  #temp
						  float(line[27:30]) / 100, #vlaznost
						  float(line[31:36]) / 10]  #tlak

danUTjednu = 6 # 1.1.2010 i 1.7.2011 je subota!
# praznici u 2011
praznici = [[1, 1], [6, 1], [24, 4], [25, 4], [1, 5], [22, 6], [23, 6],
			[25, 6], [05, 8], [15, 8], [8, 10], [1, 11], [25, 12], [26, 12]]
podaciZaPrijasnjiDan = None
for mm in range(1,13):
	brojDana = 31
	if mm in [4, 6, 9, 11]:
		brojDana = 30
	elif mm == 2:
		brojDana = 28
	for dd in range(1, brojDana + 1):
		podaciPoSatu = dict()
		try:
			filePotrosnja = open("FER_potrosnja/" + str(dd).zfill(2)+"_"+str(mm).zfill(2)+".DAT", "r")
			filePotrosnja.readline()
			filePotrosnja.readline()

			for line in filePotrosnja.readlines():
				line = line.rstrip('\n')
				if not line: continue
				(vrijeme, interval, radnaSnaga, jalovaSnaga) = line.split(',')
				radnaSnaga = float(radnaSnaga.rstrip())
				if radnaSnaga <= 0:
					continue
				sat = int(interval.rstrip()) / 4
				if sat not in podaciPoSatu:
					podaciPoSatu[sat] = [radnaSnaga, 1]
				else:
					podaciPoSatu[sat][0] += radnaSnaga
					podaciPoSatu[sat][1] += 1
			filePotrosnja.close()

		except IOError:
			print "ne potoji potrosnja za dan: {0}.{1}.{2}".format(dd,mm,gg)
			pass

		finally:
			praznik = 0
			if [dd, mm] in praznici or danUTjednu >= 6:
				praznik = 1
			for hh in range(24):
				#izracunava se potrosnja koja se predvida
				potrosnja = "NaN"
				if hh in podaciPoSatu:
					potrosnja = podaciPoSatu[hh][0] / podaciPoSatu[hh][1]
					if potrosnja == 0:
						potrosnja = "NaN"

				# izracunava se potrosnja u satu prije
				potrosnjaProsliSat = "NaN"
				prijasnjiSat = hh - 1
				if prijasnjiSat < 0:
					prijasnjiSat += 24
					if podaciZaPrijasnjiDan is not None and prijasnjiSat in podaciZaPrijasnjiDan:
						potrosnjaProsliSat = podaciZaPrijasnjiDan[prijasnjiSat][0] / podaciZaPrijasnjiDan[prijasnjiSat][1]
				else:
					if prijasnjiSat in podaciPoSatu:
						potrosnjaProsliSat = podaciPoSatu[prijasnjiSat][0] / podaciPoSatu[prijasnjiSat][1]
				if potrosnjaProsliSat == 0:
					potrosnjaProsliSat = "NaN"

				# izracunava se potrosnja dva sata prije
				potrosnjaPretprosliSat = "NaN"
				prijasnjiSat = hh - 2
				if prijasnjiSat < 0:
					prijasnjiSat += 24
					if podaciZaPrijasnjiDan is not None and prijasnjiSat in podaciZaPrijasnjiDan:
						potrosnjaPretprosliSat = podaciZaPrijasnjiDan[prijasnjiSat][0] / podaciZaPrijasnjiDan[prijasnjiSat][1]
				else:
					if prijasnjiSat in podaciPoSatu:
						potrosnjaPretprosliSat = podaciPoSatu[prijasnjiSat][0] / podaciPoSatu[prijasnjiSat][1]
				if potrosnjaPretprosliSat == 0:
					potrosnjaPretprosliSat = "NaN"

				# izracunava se potrosnja tri sata prije
				potrosnjaPrijeTriSata = "NaN"
				prijasnjiSat = hh - 3
				if prijasnjiSat < 0:
					prijasnjiSat += 24
					if podaciZaPrijasnjiDan is not None and prijasnjiSat in podaciZaPrijasnjiDan:
						potrosnjaPrijeTriSata = podaciZaPrijasnjiDan[prijasnjiSat][0] / podaciZaPrijasnjiDan[prijasnjiSat][1]
				else:
					if prijasnjiSat in podaciPoSatu:
						potrosnjaPrijeTriSata = podaciPoSatu[prijasnjiSat][0] / podaciPoSatu[prijasnjiSat][1]
				if potrosnjaPrijeTriSata == 0:
					potrosnjaPrijeTriSata = "NaN"

				# izracunaca se prosjecna i ukupna potrosnja u prijasnja 24h
				prosjek = "NaN"
				zbroj = 0
				broj = 0
				for i in range(24):
					if i < hh: #isti dan
						if i in podaciPoSatu:
							zbroj += podaciPoSatu[i][0]
							broj += podaciPoSatu[i][1]
					else: #dan prije
						if podaciZaPrijasnjiDan is not None and i in podaciZaPrijasnjiDan:
							zbroj += podaciZaPrijasnjiDan[i][0]
							broj += podaciZaPrijasnjiDan[i][1]
				if broj != 0:
					prosjek = zbroj/broj
				if zbroj == 0:
					zbroj = "NaN"

				# rezultati se zapisuju u datoteku
				key = "{:>2}{:>2}{:>2}{:>4}".format(hh + 1,dd,mm,gg) #potrebno pomaknuti radi dmhza koji racuna 1-24
				temp = podaci[key][0]
				vlaznost = podaci[key][1]
				tlak = podaci[key][2]
				fileKonacni.write(";".join(str(x) for x in [hh, praznik, mm, potrosnjaProsliSat, potrosnja]) + "\n")

			if dd == 17 and mm == 11:
				danUTjednu -= 1
			danUTjednu = danUTjednu % 7 + 1
			podaciZaPrijasnjiDan = podaciPoSatu


