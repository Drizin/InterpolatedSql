# Advanced Notes

Technical notes about the library design. For contributors or experienced developers that need to extend the library behavior.

## About Generics

This library provides different types of SQL builders for different purposes. Most builders (and some related classes) make heavy use of Generics.  

Basically you'll see the following generic types:

- `R` represents the **return type** that is built by a SQL builder. For most cases you will want to use `IInterpolatedSql` (which contains `Sql`, `SqlParameters`) or (for Dapper) `IDapperSqlCommand` (which also includes a `DbConnection`).
- `U`: represents the underlying type of a builder. This is used by the **Fluent Builder with Recursive Generics** (more about this below)

Example: if you create a class `MyCustomBuilder` that inherits from `InterpolatedSqlBuilderBase<U,R>` and builds `IInterpolatedSql`, you would define your class like this:
```cs
public class MyCustomBuilder : InterpolatedSqlBuilderBase<MyCustomBuilder, IInterpolatedSql>
{
    // ctors...

    public override IInterpolatedSql Build()
    {
        return this.AsSql();
    }
}
```

On top of that, some builders (those like `QueryBuilder` that "enrich" the output, by using some custom logic to create or modify the generated SQL statement) also use a `RB` type. `RB` is the type of builder that is used to create the return `R`. (In other words, type `RB` should implement `IBuildable<B>`, which means that it contains a method `Build()` returning a type `B`).

## Fluent Builder with Recursive Generics

Most reusable builders implement the **"Fluent Builder with Recursive Generics" design pattern**, which is a cool way of letting developers extend those classes (inherit) while still using the "Fluent API" methods from the parent classes.

In the previous example, `MyCustomBuilder` provides to the parent class (`InterpolatedSqlBuilderBase`) its own type, so all methods of the parent class that return the generic type `U` (underlying builder type) will in this case return `MyCustomBuilder`.

So basically the subclass automatically gets a lot of "Fluent" methods (method-chaining) that will always return the same type, and therefore will offer custom methods of this subclass.

Example
```cs
// InterpolatedSqlBuilderBase<U, R>
// so U (underlying type) is MyCustomBuilder
// and R (return type) is IInterpolatedSql
public class MyCustomBuilder : InterpolatedSqlBuilderBase<MyCustomBuilder, IInterpolatedSql>
{
    //...ctors, etc.

    // Custom Methods
    public MyCustomBuilder MyCustomFluentMethod(FormattableString value)
    { 
        // ...
        return this;
    }

    public IInterpolatedSql Build() // parent class defines/requires abstract R Build()
    {
        // AsSql() returns the underlying bags (stringbuilder and dictionary of parameters)
        // But you can also write your own logic here
        return base.AsSql();
    }
}

// Usage
var myBuilder = new MyCustomBuilder();
myBuilder
    .Append($"...")                     // this is defined in parent class InterpolatedSqlBuilderBase<U,R>, and will return type U (MyCustomBuilder)
    .MyCustomFluentMethod($"something") // since previous call returned MyCustomBuilder, we can call our custom method
    .Append($"...")                     // and we can still call all other methods inherited from base class
    .Build()
```
