#include <iostream>
#include <winsock2.h>
#include <process.h>

#pragma comment(lib, "ws2_32.lib")

#define BUF_SIZE 100
#define MAX_CLNT 256
#define PORT_NUM 8080

unsigned WINAPI HandleClient(void* arg);//������ �Լ�
unsigned WINAPI AnnounceMsg(void* arg);//������ �Լ�
void error(char* msg);

int clientCount = 0;
SOCKET clientSocks[MAX_CLNT];//Ŭ���̾�Ʈ ���� ������ �迭
HANDLE hMutex;//���ؽ�

static void error(char *msg)
{
	std::cout << msg;
	exit(1);
}

int main(int argc, char** argv)
{
	HANDLE hThread, AnnounceThread;
	//���� ���� �κ�
	SOCKET serverSock, clientSock;
	//���� �ּ� ����ü ����
	SOCKADDR_IN serverAddr, clientAddr;
	int clientAddrSize;
	//winsock �ʱ�ȭ ����ü ����
	WSADATA wsadata = { 0 };
	int nFlag = 0;

	//winsock �ʱ�ȭ
	nFlag = WSAStartup(MAKEWORD(2, 2), &wsadata);
	if (nFlag != 0)
		error("WSAStartup failed\n");

	hMutex = CreateMutex(NULL, FALSE, NULL);//�ϳ��� ���ؽ��� �����Ѵ�.
	
	//���� ���� �κ�
	serverSock = socket(PF_INET, SOCK_STREAM, 0);

	//���� ���� ���н�
	if (-1 == serverSock)
		error("Err : Socket create failed\n");

	//����ü �ʱ�ȭ �� �� �Ҵ�
	memset(&serverAddr, 0, sizeof(serverAddr));
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_port = htons(PORT_NUM);
	serverAddr.sin_addr.s_addr = htonl(INADDR_ANY);

	//IP �� ��Ʈ ��ȣ ����
	if (SOCKET_ERROR == bind(serverSock, (struct sockaddr*)&serverAddr, sizeof(serverAddr)))
		error("Err : cannot bind");

	//listen�� ���� Ŭ���̾�Ʈ ���� Ȯ��
	if (listen(serverSock, 5) == -1)
		error("Err : Failed listening\n");

	std::cout << "Server's waiting..." << std::endl;

	//Ŭ���̾�Ʈ ���� ���� �κ�.
	while (1)
	{
		clientAddrSize = sizeof(clientAddr);
		clientSock = accept(serverSock, (SOCKADDR*)&clientAddr, &clientAddrSize);//�������� ���޵� Ŭ���̾�Ʈ ������ clientSock�� ����
		WaitForSingleObject(hMutex, INFINITE);//���ؽ� ����
		clientSocks[clientCount++] = clientSock;//Ŭ���̾�Ʈ ���Ϲ迭�� ��� ������ ���� �ּҸ� ����
		ReleaseMutex(hMutex);//���ؽ� ����
		AnnounceThread = (HANDLE)_beginthreadex(NULL, 0, AnnounceMsg, NULL, 0, NULL);//AnnounceThread ������ ����.
		hThread = (HANDLE)_beginthreadex(NULL, 0, HandleClient, (void*)&clientSock, 0, NULL);//HandleClient ������ ����, clientSock�� �Ű������� ����
		printf("Connected Client IP : %s\n", inet_ntoa(clientAddr.sin_addr));
	}

	//socket close �� winsock clean.
	closesocket(serverSock);
	WSACleanup();

	return 0;
}

unsigned WINAPI HandleClient(void* arg){
	std::cout << "thread created. clientCount size: " << clientCount << std::endl;
	SOCKET clientSock = *((SOCKET*)arg); //�Ű������ι��� Ŭ���̾�Ʈ ������ ����
	int strLen = 0, i;
	char msg[BUF_SIZE];
	while (1) {
		//Ŭ���̾�Ʈ�κ��� �޽����� ���������� ��ٸ���.
		strLen = recv(clientSock, msg, sizeof(msg), 0);
		//Ŭ���̾�Ʈ ������ ����Ǹ� �޽����ޱ⸦ �����.
		if (strLen == 0 || strLen == -1) {
			break;
		}
		std::cout << "Client message: " << msg << std::endl;//Ŭ���̾�Ʈ�� ���� �޽��� ���.
	}
	//Ŭ���̾�Ʈ ���� �����.
	WaitForSingleObject(hMutex, INFINITE);//���ؽ� ����
	for (i = 0; i<clientCount; i++){//�迭�� ������ŭ
		if (clientSock == clientSocks[i]){//���� ���� clientSock���� �迭�� ���� ���ٸ�
			while (i++<clientCount - 1)//Ŭ���̾�Ʈ ���� ��ŭ
				clientSocks[i] = clientSocks[i + 1];//������ �����.
			break;
		}
	}
	clientCount--;//Ŭ���̾�Ʈ ���� �ϳ� ����
	std::cout << "Disconnected Client. clientCount size: " << clientCount << std::endl;
	ReleaseMutex(hMutex);//���ؽ� ����
	closesocket(clientSock);//������ �����Ѵ�.
	return 0;
}

unsigned WINAPI AnnounceMsg(void* arg) {
	char msg[BUF_SIZE];
	int i;
	while (1) {
		fgets(msg, BUF_SIZE, stdin);//�Է��� �޴´�.
		WaitForSingleObject(hMutex, INFINITE);//���ؽ� ����
		for (i = 0; i<clientCount; i++)//Ŭ���̾�Ʈ ������ŭ
			send(clientSocks[i], msg, strlen(msg), 0);//Ŭ���̾�Ʈ�鿡�� �޽����� �����Ѵ�.
		ReleaseMutex(hMutex);//���ؽ� ����
	}
}