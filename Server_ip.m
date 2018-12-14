clc
clear all
tcpipServer = tcpip('192.168.1.20',55000,'NetworkRole','Server');
fopen(tcpipServer);
i=0 ; 

i=i+1 ; 
data = membrane(1);

rawData = fread(tcpipServer,200,'char');
char(rawData(1))
for i=1:200
    rawwData(i)= char(rawData(i));
end

