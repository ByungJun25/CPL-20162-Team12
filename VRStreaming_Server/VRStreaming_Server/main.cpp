#include <iostream>
#include <winsock2.h>
#include <process.h>

#pragma comment(lib, "ws2_32.lib")

#define BUF_SIZE 100
#define MAX_CLNT 256
#define PORT_NUM 8080

unsigned WINAPI HandleClient(void* arg);//쓰레드 함수
unsigned WINAPI AnnounceMsg(void* arg);//쓰레드 함수
void error(char* msg);

int clientCount = 0;
SOCKET clientSocks[MAX_CLNT];//클라이언트 소켓 보관용 배열
HANDLE hMutex;//뮤텍스

static void error(char *msg)
{
	std::cout << msg;
	exit(1);
}

int main(int argc, char** argv)
{
	HANDLE hThread, AnnounceThread;
	//소켓 선언 부분
	SOCKET serverSock, clientSock;
	//소켓 주소 구조체 선언
	SOCKADDR_IN serverAddr, clientAddr;
	int clientAddrSize;
	//winsock 초기화 구조체 선언
	WSADATA wsadata = { 0 };
	int nFlag = 0;

	//winsock 초기화
	nFlag = WSAStartup(MAKEWORD(2, 2), &wsadata);
	if (nFlag != 0)
		error("WSAStartup failed\n");

	hMutex = CreateMutex(NULL, FALSE, NULL);//하나의 뮤텍스를 생성한다.
	
	//소켓 생성 부분
	serverSock = socket(PF_INET, SOCK_STREAM, 0);

	//소켓 생성 실패시
	if (-1 == serverSock)
		error("Err : Socket create failed\n");

	//구조체 초기화 및 값 할당
	memset(&serverAddr, 0, sizeof(serverAddr));
	serverAddr.sin_family = AF_INET;
	serverAddr.sin_port = htons(PORT_NUM);
	serverAddr.sin_addr.s_addr = htonl(INADDR_ANY);

	//IP 및 포트 번호 지정
	if (SOCKET_ERROR == bind(serverSock, (struct sockaddr*)&serverAddr, sizeof(serverAddr)))
		error("Err : cannot bind");

	//listen을 통한 클라이언트 접속 확인
	if (listen(serverSock, 5) == -1)
		error("Err : Failed listening\n");

	std::cout << "Server's waiting..." << std::endl;

	//클라이언트 접속 수락 부분.
	while (1)
	{
		clientAddrSize = sizeof(clientAddr);
		clientSock = accept(serverSock, (SOCKADDR*)&clientAddr, &clientAddrSize);//서버에게 전달된 클라이언트 소켓을 clientSock에 전달
		WaitForSingleObject(hMutex, INFINITE);//뮤텍스 실행
		clientSocks[clientCount++] = clientSock;//클라이언트 소켓배열에 방금 가져온 소켓 주소를 전달
		ReleaseMutex(hMutex);//뮤텍스 중지
		AnnounceThread = (HANDLE)_beginthreadex(NULL, 0, AnnounceMsg, NULL, 0, NULL);//AnnounceThread 쓰레드 실행.
		hThread = (HANDLE)_beginthreadex(NULL, 0, HandleClient, (void*)&clientSock, 0, NULL);//HandleClient 쓰레드 실행, clientSock을 매개변수로 전달
		printf("Connected Client IP : %s\n", inet_ntoa(clientAddr.sin_addr));
	}

	//socket close 및 winsock clean.
	closesocket(serverSock);
	WSACleanup();

	return 0;
}

unsigned WINAPI HandleClient(void* arg){
	std::cout << "thread created. clientCount size: " << clientCount << std::endl;
	SOCKET clientSock = *((SOCKET*)arg); //매개변수로받은 클라이언트 소켓을 전달
	int strLen = 0, i;
	char msg[BUF_SIZE];
	while (1) {
		//클라이언트로부터 메시지를 받을때까지 기다린다.
		strLen = recv(clientSock, msg, sizeof(msg), 0);
		//클라이언트 접속이 종료되면 메시지받기를 멈춘다.
		if (strLen == 0 || strLen == -1) {
			break;
		}
		std::cout << "Client message: " << msg << std::endl;//클라이언트로 받은 메시지 출력.
	}
	//클라이언트 접속 종료시.
	WaitForSingleObject(hMutex, INFINITE);//뮤텍스 실행
	for (i = 0; i<clientCount; i++){//배열의 갯수만큼
		if (clientSock == clientSocks[i]){//만약 현재 clientSock값이 배열의 값과 같다면
			while (i++<clientCount - 1)//클라이언트 개수 만큼
				clientSocks[i] = clientSocks[i + 1];//앞으로 땡긴다.
			break;
		}
	}
	clientCount--;//클라이언트 개수 하나 감소
	std::cout << "Disconnected Client. clientCount size: " << clientCount << std::endl;
	ReleaseMutex(hMutex);//뮤텍스 중지
	closesocket(clientSock);//소켓을 종료한다.
	return 0;
}

unsigned WINAPI AnnounceMsg(void* arg) {
	char msg[BUF_SIZE];
	int i;
	while (1) {
		fgets(msg, BUF_SIZE, stdin);//입력을 받는다.
		WaitForSingleObject(hMutex, INFINITE);//뮤텍스 실행
		for (i = 0; i<clientCount; i++)//클라이언트 개수만큼
			send(clientSocks[i], msg, strlen(msg), 0);//클라이언트들에게 메시지를 전달한다.
		ReleaseMutex(hMutex);//뮤텍스 중지
	}
}