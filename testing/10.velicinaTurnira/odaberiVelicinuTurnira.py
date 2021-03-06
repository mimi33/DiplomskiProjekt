import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp

mjerenjaPoSatima = dict()
imenaAlgoritama = dict()
for sat in range(24): #za svaki sat
	svaMjerenja = dict()
	for j in range(1, 6): #za svake postavke
		k = int(ET.parse("./"+str(j)+"/Config.xml").find("Algorithm").find("ParamK").text)
		vrijednosti = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			jedinka = batchNode.findall("Jedinka")[-1]
			vrijednosti.append(float(jedinka.get("greska")))
		svaMjerenja[k] = vrijednosti

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
plt.xlabel('velicina turnira')
plt.ylabel('Normirana Greska Jedinke')
plt.title("velicina turnira")
#x,y = zip(*sorted(zip(index, najmanji)))
p1, = plt.plot(index, najmanji)
#x,y = zip(*sorted(zip(index, najveci)))
p2, = plt.plot(index, najveci)
#x,y = zip(*sorted(zip(index, prosjek)))
p3, = plt.plot(index, prosjek)
#plt.savefig("UkupnaSlikaZaOdabirAlgoritma.png")

with open("stats.csv", "w") as dat:
	dat.write(";"+";".join(map(str, index)))
	dat.write("\n")
	dat.write("Max;")
	dat.write(";".join(map(str, najveci)))
	dat.write("\n")
	dat.write("Avg;")
	dat.write(";".join(map(str, prosjek)))
	dat.write("\n")
	dat.write("Min;")
	dat.write(";".join(map(str, najmanji)))
	dat.write("\n")


plt.show()
