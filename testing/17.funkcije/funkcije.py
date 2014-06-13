import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp
import operator as op
from mpl_toolkits.mplot3d import Axes3D
import numpy as np

def setBoxColors(bp):
	plt.setp(bp['boxes'][0], color='blue')
	plt.setp(bp['caps'][0], color='blue')
	plt.setp(bp['caps'][1], color='blue')
	plt.setp(bp['whiskers'][0], color='blue')
	plt.setp(bp['whiskers'][1], color='blue')
	plt.setp(bp['fliers'][0], color='blue')
	plt.setp(bp['fliers'][1], color='blue')
	plt.setp(bp['medians'][0], color='blue')

	plt.setp(bp['boxes'][1], color='red')
	plt.setp(bp['caps'][2], color='red')
	plt.setp(bp['caps'][3], color='red')
	plt.setp(bp['whiskers'][2], color='red')
	plt.setp(bp['whiskers'][3], color='red')
	plt.setp(bp['fliers'][2], color='red')
	plt.setp(bp['fliers'][3], color='red')
	plt.setp(bp['medians'][1], color='red')

maxGeneracija = 50
normiranje = True

greskeUcenje = dict()
greskeProvjera = dict()
for sat in range(24): #za svaki sat
	greskePoSatuUcenje = dict()
	greskePoSatuProvjera = dict()
	for j in range(12): #za svake postavke
		fun = tuple(ET.parse("./"+str(j)+"/Config.xml").find("Tree").find("FunctionSet").text.split(' '))

		greskePoSatuUcenje[fun] = []
		greskePoSatuProvjera[fun] = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			for jedinka in batchNode.find("XValidation").findall("Jedinka"):
				if int(jedinka.get("iteracija")) == maxGeneracija:
					ucenjegreska = float(jedinka.get("greska"))
					validacijaGreska= float(jedinka.get("validationError"))
					greskePoSatuUcenje[fun].append(ucenjegreska)
					greskePoSatuProvjera[fun].append(validacijaGreska)

	if normiranje:
		zbroj = sum([sum(k) for k in greskePoSatuUcenje.values()])# +sum([sum(k) for k in greskePoSatuProvjera.values()])
		broj = sum([len(k) for k in greskePoSatuUcenje.values()])#+ sum([len(k) for k in greskePoSatuProvjera.values()])
		prosjek = zbroj/broj
		for k in greskePoSatuUcenje:
			for l in range(len(greskePoSatuUcenje[k])):
				greskePoSatuUcenje[k][l] /= prosjek
			# for l in range(len(greskePoSatuProvjera[k])):
			# 	greskePoSatuProvjera[k][l] /= prosjek

	for k in greskePoSatuUcenje:
		# if k not in greskeProvjera: greskeProvjera[k] = []
		# greskeProvjera[k].extend(greskePoSatuProvjera[k])
		if k not in greskeUcenje: greskeUcenje[k] = []
		greskeUcenje[k].extend(greskePoSatuUcenje[k])

plt.figure()
plt.title("greske")
funkcije = sorted(greskeUcenje.keys(), key = lambda k: len(k))
xticks = [pp.pformat(f[5:], width=1) for f in funkcije]
y1 = [greskeUcenje[k] for k in funkcije]
plt.xticks(range(12), xticks)
box = plt.boxplot(y1, 0, '')

sortedkeys = funkcije[:]
med = [b.get_ydata()[0] for b in box["medians"]]
q3 = [b.get_ydata()[0] for b in box["boxes"]]
q1 = [b.get_ydata()[2] for b in box["boxes"]]
caps = [b.get_ydata() for b in box["caps"]]
out1 = [b[0] for b in caps[::2]]
out3 = [b[0] for b in caps[1::2]]
outDiff = map(op.sub, out1, out3)
qDiff = map(op.sub, q1, q3)

plt.figure()
ax = plt.axes()
plt.title("Greske sortirane")
x = [k for (m,k) in sorted(zip(med,sortedkeys))]
u = [greskeUcenje[k] for k in x]
bp = plt.boxplot(u, 0, '')

#plt.ylim(0,2)
ax.set_xticklabels([pp.pformat(f[5:], width=1) for f in x])
# ax.set_xticks([3*i+1.5 for i in range(len(x))])




plt.show()

# avg = [sum(greskePoDubinama[k])/len(greskePoDubinama[k]) for k in sortedkeys]
#
# fig = plt.figure()
#
# ax = fig.add_subplot(111, projection='3d')
# ax.set_title("medijan")
# X = [k[0] for k in sortedkeys]
# Y = [k[1] for k in sortedkeys]
# ax.plot_trisurf(X,Y,med, color = "blue")
# fig = plt.figure()
#
# ax = fig.add_subplot(111, projection='3d')
# ax.set_title("Q1")
# ax.plot_trisurf(X,Y, q1, color = "red")
# fig = plt.figure()
#
# ax = fig.add_subplot(111, projection='3d')
# ax.set_title("Q3")
# ax.plot_trisurf(X,Y, q3, color = "cyan")
# fig = plt.figure()
#
# ax = fig.add_subplot(111, projection='3d')
# ax.set_title("gornja granica")
# ax.plot_trisurf(X,Y, out1, color = "khaki")
# fig = plt.figure()
#
# ax = fig.add_subplot(111, projection='3d')
# ax.set_title("donja granica")
# ax.plot_trisurf(X,Y, out3, color = "yellow")
#
