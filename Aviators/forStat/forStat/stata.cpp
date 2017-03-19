#include <iostream>
#include <string>

using namespace std;

int main()
{


	while (1) {
		int  o = 0;
		cin >> o;
		string stata = "";
	int second = 0;

	cin >> stata;

	for (int i = 0; i < stata.size(); ++i)
	{
		if (stata[i] == '2')
		{
			second++;
		}
	}
	cout << o << endl;
	cout << stata.size() << " " << second << endl;
}
	return 0;
}