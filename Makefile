.PHONY: build test docker-up docker-down clean help health-check

# Default target
help:
	@echo "Westcoast Cars Development Management"
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@echo "  build         Restore and build the solution"
	@echo "  test          Run all unit tests"
	@echo "  docker-up     Start infrastructure and apps via Docker Compose"
	@echo "  docker-down   Stop and remove Docker containers"
	@echo "  clean         Remove build artifacts (bin/obj folders)"
	@echo "  health-check  Verify if all services are responding"

build:
	dotnet build westcoast-cars.sln

test:
	dotnet test westcoast-cars.sln

docker-up:
	docker compose up --build -d

docker-down:
	docker compose down

clean:
	find . -type d -name "bin" -exec rm -rf {} +
	find . -type d -name "obj" -exec rm -rf {} +

health-check:
	@echo "Checking services health..."
	@curl -s -o /dev/null -w "API: %{http_code}\n" http://localhost:5001/api/v1/vehicles/list || echo "API: DOWN"
	@curl -s -o /dev/null -w "Auth: %{http_code}\n" http://localhost:5003/api/auth/login || echo "Auth API: DOWN"
	@curl -s -o /dev/null -w "Web: %{http_code}\n" http://localhost:5005 || echo "Web: DOWN"
