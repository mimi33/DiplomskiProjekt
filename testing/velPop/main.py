import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import collections
import pprint as pp

plotati = False

glasanje = {25: 0, 50: 0, 100: 0, 250: 0, 500: 0, 750: 0, 1000: 0}
odabirSataDat = open("./Stats/ukupniStats.txt", "w")
for sat in range(24): #za svaki sat
	statsFile = open("./Stats/stats"+str(sat)+".txt", "w")
	velPop = []
	najmanji = []
	najveci = []
	prosjek = []
	svaMjerenja = dict()
	if plotati: plt.figure()
	if plotati: plt.xlabel('Velicina Populacije')
	if plotati: plt.ylabel('Greska Jedinke')
	if plotati: plt.title("Broj evaluacija = 50 000, Sat ="+str(sat))
	for j in range(1, 8): #za svake postavke
		velPop.append(int(ET.parse("./"+str(j)+"/Config.xml").find("Algorithm").find("PopulationSize").text))
		# a = int(ET.parse("./"+str(j)+"/Config.xml").find(".//Algorithm/PopulationSize").text)
		# b = int(ET.parse("./"+str(j)+"/Config.xml").find(".//Algorithm/Termination/Entry[@name='NumberOfGenerations']").text)
		#print a, b, a*b
		mjerenja = ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot()

		vrijednosti = []
		for batchNode in mjerenja.findall("Batch"):
			jedinka = batchNode.findall("Jedinka")[-1]
			vrijednosti.append(float(jedinka.get("greska")))
		svaMjerenja[velPop[-1]] = vrijednosti

		najmanji.append(min(vrijednosti))
		najveci.append(max(vrijednosti))
		prosjek.append(sum(vrijednosti)/len(vrijednosti))

		statsFile.write("\nVelicina populacije: "+str(velPop[-1])+"\n\tavg:"+str(prosjek[-1])+"\n\tmax:"+str(najveci[-1])+"\n\tmin:"+str(najmanji[-1]))
	statsFile.close()
	if plotati:
		x,y = zip(*sorted(zip(velPop, najmanji)))
		plt.plot(x,y)
		x,y = zip(*sorted(zip(velPop, najveci)))
		plt.plot(x,y)
		x,y = zip(*sorted(zip(velPop, prosjek)))
		plt.plot(x,y)
		plt.savefig("Slike/Slika"+str(sat)+".png")
	a = min(zip(najmanji, velPop))[1]
	b = min(zip(najveci, velPop))[1]
	c = min(zip(prosjek, velPop))[1]
	odabirSataDat.write("\nSat: " + str(sat) + "\n\tavg:"+str(c)+"\n\tmax:"+str(b)+"\n\tmin:"+str(a))
	d = (a + b + c)/3
	da = abs(d - a)
	db = abs(d - b)
	dc = abs(d - c)
	if da < db and da < dc:
		odabirSataDat.write("\n\t\tnajbolje:" + str(a))
		glasanje[a] += 1
	elif db < dc:
		odabirSataDat.write("\n\t\tnajbolje:" + str(b))
		glasanje[b] += 1
	else:
		odabirSataDat.write("\n\t\tnajbolje:" + str(c))
		glasanje[c] += 1

if plotati: plt.show()
od = collections.OrderedDict(sorted(glasanje.items()))
odabirSataDat.write("\nGlasanje: " + pp.pformat(glasanje))
odabirSataDat.close()