import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt

plotati = False

for i in range(24): #za svaki sat
	statsFile = open("./Stats/stats"+str(i)+".txt", "w")
	for j in range(1,6): #za svake postavke
		mutFact = float(ET.parse("./"+str(j)+"/Config.xml").getroot().find("Mutation").find("MutFactor").text)
		#print  mutFact
		mjerenja = ET.parse("./"+str(j)+"/Logovi/log"+str(i)+".txt").getroot()

		if plotati: plt.figure()
		if plotati: plt.xlabel('broj iteracija')
		if plotati: plt.ylabel('greska jedinke')
		if plotati: plt.title('Faktor mutacije = '+str(mutFact))
		zbroj = 0
		broj = 0
		najmanji = 100000
		najveci = 0
		for batchNode in mjerenja.findall("Batch"):
			x = []
			y = []
			for jedinka in batchNode.findall("Jedinka"):
				x.append(int(jedinka.get("iteracija")))
				y.append(float(jedinka.get("greska")))

			if plotati: plt.plot(x, y)
			vrijednost = y[len(y)-1]
			broj += 1
			zbroj += vrijednost
			if vrijednost < najmanji: najmanji = vrijednost
			if vrijednost > najveci: najveci = vrijednost
		statsFile.write("\nFaktor: "+str(mutFact)+"\n\tavg:"+str(zbroj/broj)+"\n\tmax:"+str(najveci)+"\n\tmin:"+str(najmanji))
	if plotati: plt.show()
	statsFile.close()
	if plotati: raw_input("Prikazani su podaci za "+str(i)+". sat.\nZa nastavak pritisnite bilo koju tipku....")
