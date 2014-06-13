import xml.etree.ElementTree as ET
import matplotlib.pyplot as plt
import pprint as pp
import operator as op
from mpl_toolkits.mplot3d import Axes3D
import numpy as np

maxGeneracija = 50

greskePoMinDubinama = dict()
greskePoMaxDubinama = dict()
greskePoDubinama = dict()
greske = dict()
for sat in range(24): #za svaki sat
	greskePoSatu = dict()
	for j in range(1, 15): #za svake postavke
		minD = int(ET.parse("./"+str(j)+"/Config.xml").find("Tree").find("MinDepth").text)
		maxD = int(ET.parse("./"+str(j)+"/Config.xml").find("Tree").find("MaxDepth").text)

		if minD not in greskePoMinDubinama:
			greskePoMinDubinama[minD] = []
		if maxD not in greskePoMaxDubinama:
			greskePoMaxDubinama[maxD] = []
		if (minD, maxD) not in greskePoDubinama:
			greskePoDubinama[(minD, maxD)] = []

		greskePoSatu[(minD,maxD)] = []
		for batchNode in ET.parse("./"+str(j)+"/Logovi/log"+str(sat)+".txt").getroot().findall("Batch"):
			for jedinka in batchNode.find("XValidation").findall("Jedinka"):
				if int(jedinka.get("iteracija")) == maxGeneracija:
					greska = float(jedinka.get("validationError"))
					greskePoMinDubinama[minD].append(greska)
					greskePoMaxDubinama[maxD].append(greska)
					greskePoDubinama[(minD, maxD)].append(greska)
					greskePoSatu[(minD, maxD)].append(greska)

	zbroj = sum([sum(k) for k in greskePoSatu.values()])
	broj = sum([len(k) for k in greskePoSatu.values()])
	prosjek = zbroj/broj
	for k in greskePoSatu:
		for l in range(len(greskePoSatu[k])):
			greskePoSatu[k][l] /= prosjek
		if k not in greske:
			greske[k] = []
		greske[k].extend(greskePoSatu[k])

plt.figure()
plt.title("greske")
dubine = sorted(greske.keys(), key = lambda k: (k[1], k[0]))
y1 = [greskePoDubinama[k] for k in dubine]
plt.xticks(range(1,15), dubine)
box = plt.boxplot(y1, 0, '')


sortedkeys = dubine[:]
med = [b.get_ydata()[0] for b in box["medians"]]
q3 = [b.get_ydata()[0] for b in box["boxes"]]
q1 = [b.get_ydata()[2] for b in box["boxes"]]
caps = [b.get_ydata() for b in box["caps"]]
out1 = [b[0] for b in caps[::2]]
out3 = [b[0] for b in caps[1::2]]
outDiff = map(op.sub, out1, out3)
qDiff = map(op.sub, q1, q3)
avg = [sum(greskePoDubinama[k])/len(greskePoDubinama[k]) for k in sortedkeys]

fig = plt.figure()

ax = fig.add_subplot(111, projection='3d')
ax.set_title("medijan")
X = [k[0] for k in sortedkeys]
Y = [k[1] for k in sortedkeys]
ax.plot_trisurf(X,Y,med, color = "blue")
fig = plt.figure()

ax = fig.add_subplot(111, projection='3d')
ax.set_title("Q1")
ax.plot_trisurf(X,Y, q1, color = "red")
fig = plt.figure()

ax = fig.add_subplot(111, projection='3d')
ax.set_title("Q3")
ax.plot_trisurf(X,Y, q3, color = "cyan")
fig = plt.figure()

ax = fig.add_subplot(111, projection='3d')
ax.set_title("gornja granica")
ax.plot_trisurf(X,Y, out1, color = "khaki")
fig = plt.figure()

ax = fig.add_subplot(111, projection='3d')
ax.set_title("donja granica")
ax.plot_trisurf(X,Y, out3, color = "yellow")





# greskeMin = {}
# greskeMax = {}
# for (minD, maxD) in greske:
# 	if minD not in greskeMin:
# 		greskeMin[minD] = []
# 	greskeMin[minD].extend(greske[(minD,maxD)])
# 	if maxD not in greskeMax:
# 		greskeMax[maxD] = []
# 	greskeMax[maxD].extend(greske[(minD,maxD)])
#
# plt.figure()
# plt.title("Greske po minimalnim dubinama")
# x = sorted(greskeMin.keys())
# y = [greskeMin[k] for k in x]
# plt.xticks(x, x)
# plt.boxplot(y, 0, '')
# plt.figure()
# plt.title("greske po maksimalnim dubinama")
# x = sorted(greskeMax.keys())
# y = [greskeMax[k] for k in x]
# plt.xticks(x, x)
# plt.boxplot(y, 0, '')



#
# plt.figure()
# plt.subplot(2,1,1)
# plt.title("Greske sortirane po medijanu")
# x = [k for (m,k) in sorted(zip(med,sortedkeys))]
# y = [greske[k] for k in x]
# plt.xticks(range(1,15), x)
# plt.boxplot(y, 0, '')
# plt.subplot(2,1,2)
# x = [k for (m,k) in sorted(zip(qDiff,sortedkeys))]
# y = [greske[k] for k in x]
# plt.xticks(range(1,15), x)
# plt.boxplot(y, 0, '')

# plt.figure()
# plt.title("Greske po minimalnim dubinama")
# x = sorted(greskePoMinDubinama.keys())
# y = [greskePoMinDubinama[k] for k in sorted(greskePoMinDubinama.keys())]
# plt.xticks(x, x)
# plt.boxplot(y, 0, '')
#
# plt.figure()
# plt.title("Greske po maksimalnim dubinama")
# x = sorted(greskePoMaxDubinama.keys())
# y = [greskePoMaxDubinama[k] for k in sorted(greskePoMaxDubinama.keys())]
# plt.xticks(x, x)
# plt.boxplot(y, 0, '')
# #
# #
# plt.figure()
# plt.title("Greske sortirane prvo po maksimalnim dubinama")
# x = sorted(greskePoDubinama.keys(), key = lambda x: (x[1], x[0]))
# y = [greskePoDubinama[k] for k in x]
# plt.xticks(range(1,15), x)
# box = plt.boxplot(y, 0, '')
# #
#
# #


# plt.figure()
# plt.title("Greske sortirane minimalnim dubinama")
# x = sorted(greskePoDubinama.keys(), key = lambda k: (k[0], k[1]))
# y = [greskePoDubinama[k] for k in x]
# plt.xticks(range(1,15), x)
# b4 = plt.boxplot(y, 0, '')
#

#
# plt.figure()
# plt.title("Greske sortirane po gornjoj granici")
# x = [k for (m,k) in sorted(zip(out1,sortedkeys))]
# y = [greskePoDubinama[k] for k in x]
# plt.xticks(range(1,15), x)
# b4 = plt.boxplot(y, 0, '')
#
# plt.figure()
# plt.title("Greske sortirane po Q1 granici")
# x = [k for (m,k) in sorted(zip(q1,sortedkeys))]
# y = [greskePoDubinama[k] for k in x]
# plt.xticks(range(1,15), x)
# b4 = plt.boxplot(y, 0, '')
#
# plt.figure()
# plt.title("Greske sortirane po Q3 granici")
# x = [k for (m,k) in sorted(zip(q3,sortedkeys))]
# y = [greskePoDubinama[k] for k in x]
# plt.xticks(range(1,15), x)
# b4 = plt.boxplot(y, 0, '')
#
# plt.figure()
# plt.title("Greske sortirane po donjoj granici")
# x = [k for (m,k) in sorted(zip(out3,sortedkeys))]
# y = [greskePoDubinama[k] for k in x]
# plt.xticks(range(1,15), x)
# b4 = plt.boxplot(y, 0, '')
#
# plt.figure()
# plt.title("Greske sortirane po razlici Q1 - Q3")
# x = [k for (m,k) in sorted(zip(qDiff,sortedkeys))]
# y = [greskePoDubinama[k] for k in x]
# plt.xticks(range(1,15), x)
# b4 = plt.boxplot(y, 0, '')
#
# plt.figure()
# plt.title("Greske sortirane po razlici gornja granica - donja granica")
# x = [k for (m,k) in sorted(zip(outDiff,sortedkeys))]
# y = [greskePoDubinama[k] for k in x]
# plt.xticks(range(1,15), x)
# b4 = plt.boxplot(y, 0, '')
#
# plt.figure()
# plt.title("Greske sortirane po srednjoj vrijednosti sa outlierima")
# x = [k for (m,k) in sorted(zip(avg,sortedkeys))]
# y = [greskePoDubinama[k] for k in x]
# plt.xticks(range(1,15), x)
# box = plt.boxplot(y)
plt.show()

def writetofile():
	with open("stats.csv", "w") as f:
		f.write(";".join(str(k) for k in sortedkeys))
		f.write("\n")
		f.write(";".join("%10.3f" % k for k in out1))
		f.write("\n")
		f.write(";".join("%10.3f" % k for k in q1))
		f.write("\n")
		f.write(";".join("%10.3f" % k for k in med))
		f.write("\n")
		f.write(";".join("%10.3f" % k for k in q3))
		f.write("\n")
		f.write(";".join("%10.3f" % k for k in out3))
		f.write("\n")