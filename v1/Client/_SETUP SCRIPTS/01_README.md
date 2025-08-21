For your container:
	docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssword!" -e "MSSQL_PID=Evaluation" -p 1433:1433  --name dbest --hostname db123 -d mcr.microsoft.com/mssql/server:2022-latest

You can connect to db with localhost port 1433, user = sa, password = P@ssword!
I am using a metadata app named Obsidion to keep track of notes: https://obsidian.md/ 