# coding=utf-8
import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import collections
import pprint as pp

plotati = False
mjerenjaPoSatima = dict()
#odabirSataDat = open("./Stats/ukupniStats.txt", "w")
for sat in range(24): #za svaki sat
	#statsFile = open("./Stats/stats"+str(sat)+".txt", "w")
	svaMjerenja = dict()
	for j in range(1, 9): #za svake postavke
		velPop = int(ET.parse("./"+str(j)+"/Config.xml").find("Algorithm").find("PopulationSize").text)
		# a = int(ET.parse("./"+str(j)+"/Config.xml").find(".//Algorithm/PopulationSize").text)
		# b = int(ET.parse("./"+str(j)+"/Config.xml").find(".//Algorithm/Termination/Entry[@name='NumberOfGenerations']").text)
		#print a, b, a*b
		vrijednosti = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			jedinka = batchNode.findall("Jedinka")[-1]
			vrijednosti.append(float(jedinka.get("greska")))
		svaMjerenja[velPop] = vrijednosti

	ukupnaGreska = sum([sum(m) for m in svaMjerenja.values()])
	brojGreski = sum([len(m) for m in svaMjerenja.values()])
	srednjaGreska = ukupnaGreska/brojGreski
	for m in svaMjerenja.values():
		for i in range(len(m)):
			m[i] /= srednjaGreska

	mjerenjaPoSatima[sat] = svaMjerenja
#mjerenjaPoSatima[sat][velPop][batchNo]

mjerenjaPoVelPop = dict()
for sat in mjerenjaPoSatima:
	for vp in mjerenjaPoSatima[sat]:
		if vp not in mjerenjaPoVelPop:
			mjerenjaPoVelPop[vp] = []
		mjerenjaPoVelPop[vp].extend(mjerenjaPoSatima[sat][vp])
#mjerenjaPoVelPop[velPop]

najmanji = []
najveci = []
prosjek = []
velicinaPopulacije = []
for vp in mjerenjaPoVelPop:
	vrijednosti = mjerenjaPoVelPop[vp]
	if vp == 25:
		print pp.pformat(vrijednosti)
	velicinaPopulacije.append(vp)
	najmanji.append(min(vrijednosti))
	najveci.append(max(vrijednosti))
	prosjek.append(sum(vrijednosti)/len(vrijednosti))

if plotati:
	plt.figure()
	plt.xlabel('Veličina Populacije')
	plt.ylabel('Normirana Greška Jedinke')
	plt.title("Broj evaluacija = 50 000")
	x,y = zip(*sorted(zip(velicinaPopulacije, najmanji)))
	plt.plot(x,y)
	x,y = zip(*sorted(zip(velicinaPopulacije, najveci)))
	plt.plot(x,y)
	x,y = zip(*sorted(zip(velicinaPopulacije, prosjek)))
	plt.plot(x,y)
	plt.savefig("Slike/UkupnaSlika.png")
	plt.show()

with open("Stats/minMaxAvg.csv", "w") as stats:
	v,y = zip(*sorted(zip(velicinaPopulacije, map(str, najveci))))
	stats.write(";".join(str(x) for x in v))
	stats.write('\n')
	stats.write(";".join(y))
	stats.write('\n')
	v,y = zip(*sorted(zip(velicinaPopulacije, map(str, prosjek))))
	stats.write(";".join(y))
	stats.write('\n')
	v,y = zip(*sorted(zip(velicinaPopulacije, map(str, najmanji))))
	stats.write(";".join(y))