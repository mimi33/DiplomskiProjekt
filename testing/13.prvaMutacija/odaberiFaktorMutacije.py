import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp

mjerenjaPoSatima = dict()
imenaAlgoritama = dict()
for sat in range(24): #za svaki sat
	svaMjerenja = dict()
	for j in range(1, 6): #za svake postavke
		k = float(ET.parse("./"+str(j)+"/Config.xml").find("Mutation").find("MutFactor").text)
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
plt.xlabel('faktor mutacije')
plt.ylabel('Normirana Greska Jedinke')
plt.title("faktor mutacije")
x,y = zip(*sorted(zip(index, najmanji)))
plt.plot(x, y)
x,y = zip(*sorted(zip(index, najveci)))
plt.plot(x, y)
x,y = zip(*sorted(zip(index, prosjek)))
plt.plot(x, y)
#plt.savefig("UkupnaSlikaZaOdabirFaktoraMutacije2.png")
plt.show()
