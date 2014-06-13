import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp

mjerenjaPoSatima = dict()
imenaAlgoritama = dict()
for sat in range(24): #za svaki sat
	svaMjerenja = dict()
	for j in range(1, 5): #za svake postavke
		ime = ET.parse("./"+str(j)+"/Config.xml").find("Algorithm").get("name")
		imenaAlgoritama[j] = ime
		vrijednosti = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			jedinka = batchNode.findall("Jedinka")[-1]
			vrijednosti.append(float(jedinka.get("greska")))
		svaMjerenja[j] = vrijednosti

	ukupnaGreska = sum([sum(m) for m in svaMjerenja.values()])
	brojGreski = sum([len(m) for m in svaMjerenja.values()])
	srednjaGreska = ukupnaGreska/brojGreski
	for m in svaMjerenja.values():
		for i in range(len(m)):
			m[i] /= srednjaGreska

	mjerenjaPoSatima[sat] = svaMjerenja
#mjerenjaPoSatima[sat][crxFact][batchNo]

mjerenjaPoVelPop = dict()
for sat in mjerenjaPoSatima:
	for vp in mjerenjaPoSatima[sat]:
		if vp not in mjerenjaPoVelPop:
			mjerenjaPoVelPop[vp] = []
		mjerenjaPoVelPop[vp].extend(mjerenjaPoSatima[sat][vp])
#mjerenjaPoVelPop[crxFact]

najmanji = []
najveci = []
prosjek = []
index = []
for vp in mjerenjaPoVelPop:
	vrijednosti = mjerenjaPoVelPop[vp]
	if vp == 25:
		print pp.pformat(vrijednosti)
	index.append(vp)
	najmanji.append(min(vrijednosti))
	najveci.append(max(vrijednosti))
	prosjek.append(sum(vrijednosti)/len(vrijednosti))

plt.figure()
plt.xlabel('Velicina Populacije')
plt.ylabel('Normirana Greska Jedinke')
plt.title("usporedba algoritama")
x,y = zip(*sorted(zip(index, najmanji)))
plt.plot(x, y,"bo")
x,y = zip(*sorted(zip(index, najveci)))
plt.plot(x, y, "ro")
x,y = zip(*sorted(zip(index, prosjek)))
plt.plot(x, y, "go")
pp.pprint(imenaAlgoritama)
plt.xticks( range(1,5), ('Eliminacijski2', 'Eliminacijski1', 'Generacijski1', 'Generacijski2') )
#plt.savefig("UkupnaSlikaZaOdabirAlgoritma.png")
plt.show()


with open("stats.csv" ,"w") as dat:
	dat.write(";".join([imenaAlgoritama[i] for i in index]))
	dat.write("\n")
	dat.write(";".join(map(str, najveci)))
	dat.write("\n")
	dat.write(";".join(map(str, prosjek)))
	dat.write("\n")
	dat.write(";".join(map(str, najmanji)))
	dat.write("\n")