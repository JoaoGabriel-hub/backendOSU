// Architecture and Software Engineering Principles //

This project was designed following basic software engineering principles, with a clear focus on n-layer architecture and SOLID principles, adapted to the scope of a RESTful Web API.

The domain of the project is based on an idea of a library called "Biblioteca" in portuguese. We have 
classes such as the users, the books, the author and the loans.

==> N-Layer Architecture

The application is organized into well-defined layers, each with a clear responsibility:

>> Controllers layer (Controllers/)

        Responsible for handling HTTP requests and responses.

        Exposes REST endpoints (POST, GET, PUT, DELETE).

        Does not contain database logic.

        Delegates persistence to the data layer via DbContext.

>> Models layer (Models/)

        Represents domain entities (User, Author, Book, Loan, BookAuthor).

        Maps directly to database tables using Entity Framework Core.

        Contains no business logic or infrastructure concerns.

>> Data layer (Data/)

        Contains AppDbContext.

        Responsible for database access and entity persistence.

        Isolated from HTTP and authentication concerns.

>> Authentication layer (Auth/)

        Handles user authentication and JWT generation.

        Centralizes login logic and token creation.

        Keeps authentication concerns separate from business entities.


==> SOLID Principles

The project follows the core ideas of the SOLID principles, within the context of a Web API:

>> Single Responsibility Principle (SRP)

    Each class has one clear responsibility:
        Controllers handle HTTP flow only.
        Models represent data structures.
        AppDbContext manages database access.
        Authentication logic is isolated in the Auth module.

>> Open/Closed Principle (OCP)

    The system can be extended (new endpoints, new entities, new rules) without modifying existing core logic.

>> Liskov Substitution Principle (LSP)

    The application relies on framework abstractions (ControllerBase, DbContext) without violating expected behavior.

>> Interface Segregation Principle (ISP)

    Controllers expose only the endpoints they need, avoiding unnecessary methods or large shared interfaces.

>> Dependency Inversion Principle (DIP)

    Dependencies such as AppDbContext and IConfiguration are injected via Dependency Injection, rather than being instantiated directly inside classes.


==> Authentication and Authorization

    Authentication is handled using JWT (JSON Web Tokens).
    Authorization is applied selectively using [Authorize] attributes.
    Endpoints that modify sensitive data (e.g. DELETE operations) require a valid token.
    This ensures secure access while keeping read operations flexible where appropriate.