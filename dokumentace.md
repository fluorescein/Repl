# Zápočtový program z Programování 2 — REPL

Tento zápočtový program představuje interpreter se zabudovaným **REPL** (Read-Eval-Print-Loop) pro jednoduchý programovací jazyk.

Po spuštění programu se otevře konzolové okno s promptem, kam je možné psát vstup. Vstup není možné rozdělit na více řádek.
Po stisknutí klávesy Enter se zadaný kód vyhodnotí a zobrazí se případný výsledek.

# Popis REPL
Jazyk podporovaný tímto interpretem je imperativní, dynamicky typovaný s následujícmi typy: `int`, `bool`, `string`.

- typ `int` odpovídá typu `int` v jazyce C#, tj. 32-bitový signed integer
- typ `string` představuje řetězce odpovídající typu `string` v jazyce C#
- typ `bool` představuje dvě booleovské hodnoty: `true` a `false`

- jazyk je tvořen posloupností příkazů, každý příkaz musí končit středníkem
- interpreter nepodporuje scope, takže všechny proměnné jsou globální


### Aritmetika
- základní aritmetické operace: `+, -, *, /, ^` s ohledem na prioritu operací
- operátor `**` který je ekvivalentní operátoru `^` (tj. umocňování)

```cs
[repl] print 1 + 2 * 3;
7
```

### Logické operátory **`and`** a **`or`**

```cs
[repl] true and false;
False
[repl] true or false;
True
```
### Proměnné
- k deklaraci slouží klíčové slovo `let`
- názvy proměnných musí začínat písmenem a můžou obsahovat číslice a podtržítko

```cs
let x = 10;
let y2 = true;
let z_v = "literal";
```

- je možné oddělit deklaraci a inicializaci proměnných
```cs
let x;
x = 0;
```

### Funkce `print`
- vypíše hodnotu dané proměnné/výrazu
- jedná se o příkaz, tudíž musí být ukončen středníkem

```cs
[repl] let x = 1;
[repl] print x;
1
```
### Konkatenace řetězců
- operátor "+" může být použit pro konkatenaci řetězců

```cs
[repl] print "hello" + " " + "world";
hello world
```

### `if/then/else` výraz
- `else` větev není povinná
- za klíčovým slovem `then`, resp. `else` může následovat jenom jeden příkaz

```cs
[repl] let x = 1;
[repl] let y = 10;
[repl] if x < y then print "x < y"; else print "x >= y";
```

### `for` cyklus
- `for i in a..b` je ekvivalentní `for _ in range(a, b)` v Pythonu
- písmeno `i` v tomto příkladě nereprezentuje proměnnou, jde jenom o identifikátor bez významu (v průběhu cyklu neexistuje žádná proměnná "`i`" přes kterou by se iterovalo)
- výrazy na levé a pravé straně symbolu "`..`" se musí vyhodnotit na číselný typ
- tělo cyklu může obsahovat víc příkazů a musí být vymezeno klíčovými slovy `begin` a `end`

```cs
[repl] let x = 0;
[repl] for i in 1..5 begin print x; x = x + 1; end
0
1
2
3
```

### `while` cyklus
- stejně jako v případě for-cyklu musí být tělo while-cyklu vymezeno slovy `begin` a `end`
- podmínka while-cyklu není ohraničena závorkami

```cs
[repl] let x = 1;
[repl] while x < 5 begin print x; x = x + 1; end
1
2
3
4
```

### Komentáře
- jsou podporovány jednořádkové komentáře

```cs
[repl] // komentar
[repl]
```

## Gramatika jazyka

```cs
program     ::= declaration* EOF ;

declaration ::= variableDeclaration
              | statement ;
             
statement   ::= expressionStatement
              | printStatement ;

statement   ::= expressionStatement
              | printStatement
              | ifStatement
              | forStatement
              | whileStatement ;

variableDeclaration ::= "let" IDENTIFIER ( "=" expression )? ";" ;

expressionStatement ::= expression ";" ;

printStatement      ::= "print" expression ";" ;

ifStatement         ::= "if" expression "then" statement ( "else" statement )? ;

forStatement        ::= "for" .* "in" expression ".." expression "begin" (statement)* "end" ;

whileStatement      ::= "while" expression "begin" (statement)* "end" ;

expression  ::= literal
              | unary
              | binary
              | grouping
              | assignment

assignment  ::= IDENTIFIER "=" assignment
              | equality ;

logicalOr   ::= logicalAnd ( "or" logicalAnd )* ;
logicalAnd  ::= equality ( "and" equality )* ;

literal     ::= NUMBER | STRING | "true" | "false" ;
grouping    ::= "(" expression ")" ;
unary       ::= ( "-" | "!" ) expression ;
binary      ::= expression operator expression ;
operator    ::= "+" | "-" | "*" | "/" | "^" | "==" | "!=" | "<" | "<=" | ">" | ">=" | AND | OR ;

expression  ::= equality ;
equality    ::= comparison (( "!=" | "==" ) comparison)* ;
comparison  ::= term (( ">" | ">=" | "<" | "<=" ) term)* ;
term        ::= factor (( "+" | "-" ) factor)* ;
factor      ::= unary ( ( "*" | "/" ) unary)* ;
exponent    ::= unary ( "^" unary)* ;
unary       ::= ( "!" | "-" ) unary | primary ;
primary     ::= NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER;

```

<br>

# Organizace

Program se skládá z několika tříd — Token, Lexer, Parser, Expression, Statement, Interpreter, Environment a Error.

## Třída **`Token`**

Třída Token reprezentuje základní "jednotku" zdrojového kódu — token. Třída `Token` obsahuje čtyři atributy:

- `TokenType type`
    - druh tokenu
- `string lexeme`
    - textová reprezentace daného tokenu (například v případě čísla 123 ve zdrojovém kódu bude jeho textová reprezentace "123")
- `object value`
    - hodnota tokenu odpovídajícího typu (`int`, `bool`, `string`)

Všechny druhy tokenů jsou definovány enumerací `public enum TokenType`.
<br>

## Třída **`Lexer`**

Lexer je část programu, která je zodpovědná za lexikální analýzu zdrojového kódu – tedy rozdělení zdrojového kódu na posloupnost tokenů.<br>
Jednotlivými tokeny jsou klíčová slova, operátory, identifikátory, literály a oddělovače.

Všechna klíčová slova jsou definována ve `Dictionary<string, TokenType> keywords`, kde klíčem je dané klíčové slovo.

Lexer je inicializován řetězcem zadaným do konzole.

Atribut `int start` označuje pozici ve vstupném textu od které se začíná hledání dalšího tokenu, atribut `int current` označuje konečnou pozici. Podřetězec vytyčen těmito pozicemi představuje nalezený token.

K lexikální analýze celého vstupu slouží metoda `Tokenize()`. Výstupem lexeru je seznam tokenů.
<br>

## Třída **`Expression`**

Třída ***Expression*** je základem pro implementaci výrazů — konstruktů, které se vyhodnocují na hodnotu.

Všechny výrazy kter0 existují v implementovaném jazyce, jsou reprezentovány třídou odvozenou od abstraktní třídy ***Expression***.
<br>

V implementaci této třídy byl využit *Visitor/Interpreter pattern* který později umožní interpreteru zavolat metodu pro vyhodnocení podle druhu výrazu.

Třída `Expression` deklaruje abstraktní metodu `Accept()`, kterou bude muset implementovat každá třída odvozená od třídy `Expression`.

Výrazy jsou následující:
- **literály** — čísla, řetězce, booleovské hodnoty
- **unární výrazy** — operace s jedním operandem: logická negace (`!`) a aritmetická negace (`-`)
- **binární výrazy** — operaci se dvěma operandy — aritmetické operace, operace porovnávání
- **logické výrazy** — vyhodnocují se na `true`/`false`
- **seskupovací výrazy** — kombinace předchozích výrazů
<br>

## Třída **`Statement`**

Zdrojový kód je posloupností příkazů. Tyto příkazy jsou reprezentovány třídou `Statement`.

Tato třída je velice podobná tříde `Expression` — všechny třídy reprezentující jednotlivé příkazy jsou odvozeny od abstraktní třídy `Statement`.

Příkazy jsou:
- **print**
- **variable statement** — deklarace a inicializace proměnné
- **expression statement** — přiřazení hodnoty do proměnné
- **if statement** — `if/then/else`
- **for statement** — `for` cyklus
- **while statement** — `while` cyklus
<br>

## Třída **`Parser`**

Dalším krokem je zpracování seznamu tokenů parserem, který sestaví **abstraktní syntaktický strom** (AST).

Pro účely tohoto programu byl implementován parser s rekurzivním sestupem. Parser je inicializován seznamem tokenů, který je výstupem lexeru.

Princip parseru je následující: pro každé pravidlo v gramatice jazyka bude existovat metoda, která parsuje dané pravidlo. Pokud najde token který je součástí daného pravidla, vytvoří odpovídající uzel/uzly v AST.
Pokud ne, rekurzivně zavolá další metodu.
Metody se volají podle priority jednotlivých pravidel — od pravidla s nejnižší prioritou k pravidlu s nejvyšší prioritou.

Samotné parsování se spouští metodou `Parse()`.

Jako první se parsují příkazy, konkrétně deklarace, následují příkazy `if`, `print`, `for` a `while`.
<br>
Následuje parsování výrazů podle priorit. Výstupem parseru je seznam příkazů.
<br>

## Třída **`Interpreter`**

Interpreter je zodpovědný za vyhodnocení výrazů a vykonání příkazů podle AST sestaveného parserem.

Ve třídě `Interpreter` jsou implementovány všechny `Visit` metody deklarované ve třídách `Expression` a `Statement`.
<br>

## Třída `Environment`

Tato třída definuje metody pro manipulaci s proměnnými.

Všechny proměnné deklarovány za běhu REPL jsou uloženy ve slovníku, klíčem jsou názvy proměnných.

Instance třídy `Environment` je vytvořena jenom jednou a existuje po celou dobu běhu interpreteru.
<br>

## Třída `Error`

Třída `Error` definuje `ParserError` a `RuntimeError`, které jsou odvozeny od třídy `Exception`.
