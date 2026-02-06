.PHONY: tools-restore format clean rebuild

tools-restore:
	dotnet tool restore
format:
	cd src && dotnet csharpier format .
clean:
	dotnet clean
rebuild: clean
	dotnet build --no-incremental
