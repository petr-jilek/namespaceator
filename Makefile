.PHONY: tools-restore format clean rebuild nuget-publish

tools-restore:
	dotnet tool restore
format:
	cd src && dotnet csharpier format .
clean:
	dotnet clean
rebuild: clean
	dotnet build --no-incremental
make nuget-publish:
	cd scripts && sh nuget-publish.sh
