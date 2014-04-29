
gg = 2011
podaci = dict() # key je hhddmmyyyy, nema vodecih nula nego su praznine
fileKonacni = open("potrosnja.csv", "w")
fileKonacni.write("Sat;Praznik;Mjesec;Temperatura;PrethodnaPotrosnja;Potrosnja\n")
fileVrijeme = open("ZGM_satni.txt", "r").readlines()
fileVrijeme.pop(0)
fileVrijeme.pop(0)
fileVrijeme.pop(0)
for line in fileVrijeme:
	if not line.rstrip('\n'): continue
	podaci[line[0:10]] = [float(line[22:26]) / 10,  #temp
						  float(line[27:30]) / 100, #vlaznost
						  float(line[31:36]) / 10]  #tlak

danUTjednu = 6 # 1.1.2011 je subota
# praznici u 2011
praznici = [[1, 1], [6, 1], [24, 4], [25, 4], [1, 5], [22, 6], [23, 6],
			[25, 6], [05, 8], [15, 8], [8, 10], [1, 11], [25, 12], [26, 12]]
podaciZaPrijasnjiDan = None
for mm in range(1,6): #TODO postaviti za sve mjesece
	brojDana = 31
	if mm in [4, 6, 9, 11]:
		brojDana = 30
	elif mm == 2: #TODO kaj sa drugim godinama?
		brojDana = 28
	for dd in range(1, brojDana + 1):
		podaciPoSatu = dict()
		try:
			filePotrosnja = open("potrosnja/" + str(dd).zfill(2)+"_"+str(mm).zfill(2)+".DAT", "r")
			filePotrosnja.readline()
			filePotrosnja.readline()

			for line in filePotrosnja.readlines():
				line = line.rstrip('\n')
				if not line: continue
				(vrijeme, interval, radnaSnaga, jalovaSnaga) = line.split(',')
				sat = int(interval.rstrip()) / 4
				if sat not in podaciPoSatu:
					podaciPoSatu[sat] = [float(radnaSnaga.rstrip()), 1]
				else:
					podaciPoSatu[sat][0] += float(radnaSnaga.rstrip())
					podaciPoSatu[sat][1] += 1
			filePotrosnja.close()

		except IOError:
			pass

		finally:
			praznik = 0
			if [dd, mm] in praznici or danUTjednu >= 6:
				praznik = 1
			for hh in range(24):
				potrosnja = "NaN"
				if hh in podaciPoSatu:
					potrosnja = podaciPoSatu[hh][0] / podaciPoSatu[hh][1]
					if potrosnja == 0:
						potrosnja = "NaN"

				prijasnjaPotrosnja = "NaN"
				prijasnjiSat = hh - 1
				if prijasnjiSat < 0:
					prijasnjiSat = 23
					if podaciZaPrijasnjiDan is not None and prijasnjiSat in podaciZaPrijasnjiDan:
						prijasnjaPotrosnja = podaciZaPrijasnjiDan[prijasnjiSat][0] / podaciZaPrijasnjiDan[prijasnjiSat][1]
				else:
					if prijasnjiSat in podaciPoSatu:
						prijasnjaPotrosnja = podaciPoSatu[prijasnjiSat][0] / podaciPoSatu[prijasnjiSat][1]
						if prijasnjaPotrosnja == 0:
							prijasnjaPotrosnja = "NaN"
				key = "{:>2}{:>2}{:>2}{:>4}".format(hh + 1,dd,mm,gg) #potrebno pomaknuti radi dmhza koji racuna 1-24
				fileKonacni.write(";".join(str(x) for x in [hh, praznik, mm, podaci[key][0], prijasnjaPotrosnja, potrosnja]) + "\n")
				# podaci[key][0] -> temperatura
				#fileKonacni.write("{};{};{};{};{};{}\n".format(hh, dd, mm, podaci[key][0], potrosnja))

			danUTjednu = danUTjednu % 7 + 1
			podaciZaPrijasnjiDan = podaciPoSatu


