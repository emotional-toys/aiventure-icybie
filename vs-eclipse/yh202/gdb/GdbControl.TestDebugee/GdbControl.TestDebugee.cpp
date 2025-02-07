// GdbControl.TestDebugee.cpp : Defines the entry point for the console application.
//

// for tests with debug info compile it like (use static linking as suggested in order to circumvent dll search-path problems)
// g++ -static ..\..\GdbControl.TestDebugee\GdbControl.TestDebugee.cpp -o GdbControl.TestDebugee.exe

#ifdef VisualStudioBuild
#include "stdafx.h"
#endif

#include <iostream>

using std::cout;
using std::cerr;
using std::endl;

static int globalVar1 = 0x01234567;
static double globalVar2 = 666.666;
static char globalVar3[] = { 0x10, 0x20, 0x30, 0x40 };

int Function1()
{
	return 17;
}

int Function2()
{
	return 25;
}

void ShowSomeOutputOnStdout()
{
	cout << "Some Output On Stdout" << endl;
}

void ShowSomeOutputOnStderr()
{
	cout << "Some Output On Stderr" << endl;
}

int main(int argc, char* argv[])
{
	Function1();
	Function2();
	ShowSomeOutputOnStdout();
	ShowSomeOutputOnStderr();
	return 0;
}

