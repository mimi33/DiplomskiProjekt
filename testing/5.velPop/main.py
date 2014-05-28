import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import collections
import pprint as pp

plotati = True
mjerenjaPoSatima = dict()
#odabirSataDat = open("./Stats/ukupniStats.txt", "w")
for sat in range(24): #za svaki sat
	#statsFile = open("./Stats/stats"+str(sat)+".txt", "w")
	svaMjerenja = dict()
	for j in range(1, 8): #za svake postavke
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
	plt.xlabel('Velicina Populacije')
	plt.ylabel('Normirana Greska Jedinke')
	plt.title("Broj evaluacija = 50 000")
	x,y = zip(*sorted(zip(velicinaPopulacije, najmanji)))
	plt.plot(x,y)
	x,y = zip(*sorted(zip(velicinaPopulacije, najveci)))
	plt.plot(x,y)
	x,y = zip(*sorted(zip(velicinaPopulacije, prosjek)))
	plt.plot(x,y)
	plt.savefig("Slike/UkupnaSlika.png")
	plt.show()

# 	statsFile.close()
# 	if plotati:
# 		x,y = zip(*sorted(zip(velPop, najmanji)))
# 		plt.plot(x,y)
# 		x,y = zip(*sorted(zip(velPop, najveci)))
# 		plt.plot(x,y)
# 		x,y = zip(*sorted(zip(velPop, prosjek)))
# 		plt.plot(x,y)
# 		plt.savefig("Slike/Slika"+str(sat)+".png")
# 	a = min(zip(najmanji, velPop))[1]
# 	b = min(zip(najveci, velPop))[1]
# 	c = min(zip(prosjek, velPop))[1]
# 	odabirSataDat.write("\nSat: " + str(sat) + "\n\tavg:"+str(c)+"\n\tmax:"+str(b)+"\n\tmin:"+str(a))
# 	d = (a + b + c)/3
# 	da = abs(d - a)
# 	db = abs(d - b)
# 	dc = abs(d - c)
# 	if da < db and da < dc:
# 		odabirSataDat.write("\n\t\tnajbolje:" + str(a))
# 		glasanje[a] += 1
# 	elif db < dc:
# 		odabirSataDat.write("\n\t\tnajbolje:" + str(b))
# 		glasanje[b] += 1
# 	else:
# 		odabirSataDat.write("\n\t\tnajbolje:" + str(c))
# 		glasanje[c] += 1
#
# if plotati: plt.show()
# od = collections.OrderedDict(sorted(glasanje.items()))
# odabirSataDat.write("\nGlasanje: " + pp.pformat(glasanje))
# odabirSataDat.close()