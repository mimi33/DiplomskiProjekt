
gg = 2011
podaci = dict() # key je hhddmmyyyy, nema vodecih nula nego su praznine
fileKonacni = open("potrosnja.csv", "w")
fileKonacni.write("Sat;DanUTjednu;Mjesec;Temperatura;Potrosnja\n")
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
for mm in range(1,6): #TODO postaviti za sve mjesece
	brojDana = 31
	if mm in [4, 6, 9, 11]:
		brojDana = 30
	elif mm == 2:
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
			for hh in range(24):
				key = "{:>2}{:>2}{:>2}{:>4}".format(hh + 1,dd,mm,gg) #todo kako to srediti sa satima?
				potrosnja = "NaN"
				if hh in podaciPoSatu:
					potrosnja = podaciPoSatu[hh][0] / podaciPoSatu[hh][1]
					if potrosnja == 0:
						potrosnja = "NaN"
				fileKonacni.write(";".join(str(x) for x in [hh, danUTjednu, mm, podaci[key][0], potrosnja]) + "\n")
				# podaci[key][0] -> temperatura
				#fileKonacni.write("{};{};{};{};{};{}\n".format(hh, dd, mm, podaci[key][0], potrosnja))

			danUTjednu = danUTjednu % 7 + 1


