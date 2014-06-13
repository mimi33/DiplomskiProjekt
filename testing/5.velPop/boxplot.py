# coding=utf-8
import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import collections
import pprint as pp
from scipy.stats import gaussian_kde
from numpy import arange

def violin_plot(ax,data,pos, bp=False):
	'''
	create violin plots on an axis
	'''
	dist = max(pos)-min(pos)
	w = min(0.15*max(dist,1.0),0.5)
	for d,p in zip(data,pos):
		k = gaussian_kde(d) #calculates the kernel density
		m = k.dataset.min() #lower bound of violin
		M = k.dataset.max() #upper bound of violin
		x = arange(m,M,(M-m)/100.) # support for violin
		v = k.evaluate(x) #violin profile (density curve)
		v = v/v.max()*w #scaling the violin to the available space
		ax.fill_betweenx(x,p,v+p,facecolor='y',alpha=0.3)
		ax.fill_betweenx(x,p,-v+p,facecolor='y',alpha=0.3)
	if bp:
		ax.boxplot(data,notch=1,positions=pos,vert=1)

plotati = True

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

plt.figure()
plt.xlabel('Velicina Populacije')
plt.ylabel('Normirana Greska Jedinke')
plt.title("Broj evaluacija = 50 000")
x = sorted(mjerenjaPoVelPop.keys())
y = [mjerenjaPoVelPop[i] for i in x]
plt.xticks(x,x)
plt.boxplot(y, 0, '')

plt.figure()
plt.xlabel('Velicina Populacije')
plt.ylabel('Normirana Greska Jedinke')
plt.title("Broj evaluacija = 50 000")
plt.xticks(x,x)
plt.boxplot(y)
plt.show()


