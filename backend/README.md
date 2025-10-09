dotnet ef migrations add InitialCreate
dotnet ef database update
docker-compose up -d
docker ps
docker-compose down
git init
git add .
git commit -m "add otp authentication feature"
git remote add origin https://github.com/Hung0Toy2810/Do-an-IT-Web.git
git branch -M main
git push -u origin main
dotnet ef database drop
