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
	//winsock �ʱ�ȭ ����ü ����
	WSADATA wsadata = { 0 };
	int nFlag = 0;

	//winsock �ʱ�ȭ
	nFlag = WSAStartup(MAKEWORD(2, 2), &wsadata);
	if (nFlag != 0)
		error("WSAStartup failed\n");

	//���� ���� �κ�
	int		sock;
	sock = socket(PF_INET, SOCK_STREAM, 0);

	//���� ���� ���н�
	if (-1 == sock)
		error("Err : Socket create failed\n");

	//���� �ּ� ����ü ����
	struct sockaddr_in	server_addr;

	//����ü �ʱ�ȭ �� �� �Ҵ�
	memset(&server_addr, 0, sizeof(server_addr));
	server_addr.sin_family = AF_INET;
	server_addr.sin_port = htons(8080);
	server_addr.sin_addr.s_addr = htonl(INADDR_ANY);

	//IP �� ��Ʈ ��ȣ ����
	if (-1 == bind(sock, (struct sockaddr*)&server_addr, sizeof(server_addr)))
		error("Err : cannot bind");

	//listen�� ���� Ŭ���̾�Ʈ ���� Ȯ��
	if (listen(sock, 5) == -1)
		error("Err : Failed listening\n");

	std::cout << "Server's waiting..." << std::endl;

	//Ŭ���̾�Ʈ ���� ���� �κ�.
	while (1)
	{
		int size = sizeof(server_addr);
		SOCKET client_sock = accept(sock, (struct sockaddr*)&server_addr, &size);

		std::cout << "Connected Client : " << std::endl;
	}

	//socket close �� winsock clean.
	closesocket(sock);
	WSACleanup();

	return 0;
}