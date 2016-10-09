#include <iostream>
#include <winsock2.h>

#pragma comment(lib, "ws2_32.lib")

static void error(char *msg)
{
	std::cout << msg;
	exit(1);
}

int main(int argc, char** argv)
{
	//winsock 초기화 구조체 선언
	WSADATA wsadata = { 0 };
	int nFlag = 0;

	//winsock 초기화
	nFlag = WSAStartup(MAKEWORD(2, 2), &wsadata);
	if (nFlag != 0)
		error("WSAStartup failed\n");

	//소켓 생성 부분
	int		sock;
	sock = socket(PF_INET, SOCK_STREAM, 0);

	//소켓 생성 실패시
	if (-1 == sock)
		error("Err : Socket create failed\n");

	//소켓 주소 구조체 선언
	struct sockaddr_in	server_addr;

	//구조체 초기화 및 값 할당
	memset(&server_addr, 0, sizeof(server_addr));
	server_addr.sin_family = AF_INET;
	server_addr.sin_port = htons(8080);
	server_addr.sin_addr.s_addr = htonl(INADDR_ANY);

	//IP 및 포트 번호 지정
	if (-1 == bind(sock, (struct sockaddr*)&server_addr, sizeof(server_addr)))
		error("Err : cannot bind");

	//listen을 통한 클라이언트 접속 확인
	if (listen(sock, 5) == -1)
		error("Err : Failed listening\n");

	std::cout << "Server's waiting..." << std::endl;

	//클라이언트 접속 수락 부분.
	while (1)
	{
		int size = sizeof(server_addr);
		SOCKET client_sock = accept(sock, (struct sockaddr*)&server_addr, &size);

		std::cout << "Connected Client : " << std::endl;
	}

	//socket close 및 winsock clean.
	closesocket(sock);
	WSACleanup();

	return 0;
}