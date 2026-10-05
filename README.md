<p align="center">
<img width="1000" height="431" alt="Image" src="https://github.com/user-attachments/assets/96a26a20-18e9-48dd-98c4-f1de11abba35" />
</p>

<p align="center">
  <a href="https://github.com/[ТВОЙ_ЛОГИН]/[ИМЯ_РЕПО]/releases">
    <img src="https://img.shields.io/badge/Version-v1.0.0-brightgreen.svg?style=flat-square" alt="Latest Version" />
  </a>
  <a href="https://github.com/[ТВОЙ_ЛОГИН]/[ИМЯ_РЕПО]/actions">
    <img src="https://img.shields.io/badge/Build-passing-brightgreen.svg?style=flat-square" alt="Build Status" />
  </a>
  <a href="https://www.python.org/downloads/">
    <img src="https://img.shields.io/badge/Python-3.10%2B-blue.svg?style=flat-square" alt="Python Version" />
  </a>
  <a href="LICENSE">
    <img src="https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square" alt="License" />
  </a>
  <a href="https://github.com/[ТВОЙ_ЛОГИН]">
    <img src="https://img.shields.io/badge/Author-[dinosaurguk]-orange.svg?style=flat-square" alt="Author" />
  </a>
  <a href="https://t.me/[ИМЯ_ТВОЕГО_БОТА]">
    <img src="https://img.shields.io/badge/Telegram-Bot-blue.svg?style=flat-square" alt="Telegram Bot" />
  </a>
</p>
# Marketplace_MDK

# Маркетплейс на Windows Forms + MS Access

Студенческий проект: десктопное приложение для онлайн-магазина одежды.
Полная реализация по диаграмме![Uploading изображение.png…]()
 — Use Case.

## Why I wanted to make it?

Потому что выбрал такой проект и его надо было сделать ну и потому что мы крутая команда

Но мы справились. В проекте есть всё, что нарисовано на диаграммах: корзина, оформление заказа,
оплата, сборка, назначение курьера, изменение статуса. И всё это работает.

Не ставьте 0, 1, 2, 3, 4. Только 5, пожалуйста, и автомат :)

## What can it do?

### 🛍 Покупатель
- Просмотр каталога товаров
- Поиск и фильтрация по названию, артикулу, цвету
- Карточка товара (двойной клик по строке) — с «картинкой»-заглушкой
- Корзина: добавление, увеличение количества, удаление
- Оформление заказа: адрес доставки, телефон, способ оплаты
- Автоматический расчёт стоимости доставки и итоговой суммы
- Просмотр своих заказов и их статусов

### 📦 Продавец
- Видит **только свои** товары
- Добавление товара с указанием количества на складе
  (`id_seller` подставляется автоматически из сессии)
- Сборка заказа с проверкой наличия товара на складе
- Назначение курьера на заказ
- Видит только те заказы, где есть хотя бы один его товар
- Состав заказа и доставка — тоже только по своим заказам

### 🚚 Курьер
- Видит только назначенные на него заказы
- Изменяет статус заказа по цепочке:
  «В сборке» → «Доставляется» → «Выполнен»
- Видит доставку только по своим заказам

## Usage

1. **Запуск**: открываешь `Vulpes0.slnx` в Visual Studio, жмёшь F5.
2. **Авторизация**: вводишь логин и пароль (или регистрируешься на вкладке «Регистрация»).
3. **Покупатель**: добавляешь товары в корзину через карточку товара,
   переходишь на вкладку «Корзина», жмёшь «Оформить заказ»,
   заполняешь адрес и телефон, выбираешь способ оплаты, подтверждаешь.
4. **Продавец**: на вкладке «Заказы» выбираешь свой заказ,
   жмёшь «Собрать заказ» (статус меняется на «В сборке»),
   потом «Назначить курьера» и выбираешь курьера из списка.
5. **Курьер**: на вкладке «Заказы» выбираешь заказ и жмёшь
   «Обновить статус» — статус двигается по цепочке.

## Installation

1. Клонируй репозиторий:

git clone https://github.com/Dinosaurguk/Marketplace_MDK.git

2. Открой `Vulpes0.slnx` в Visual Studio
3. Убедись, что `Маркетплейс.accdb` лежит в корне проекта
4. F5

## Test Accounts

| Логин | Пароль | Роль       |
|-------|--------|------------|
| ivan  | 123    | Покупатель |
| OOO   | 123    | Продавец   |
| Igor  | 123    | Курьер     |

## Functionality & Libraries

- **C# / .NET Framework** — язык и платформа
- **Windows Forms** — интерфейс
- **MS Access + OleDb** — база данных
- **DataSet / BindingSource** — работа с данными
- **System.Data.OleDb** — запросы к Access

## Implementation Notes

- Номер заказа генерируется автоматически в формате `MP-2026-XXX`
- При оформлении заказа товары списываются со склада (`stock - kolvo`)
- После успешной оплаты корзина очищается
- Перед сборкой заказа проверяется наличие товара на складе
- Стоимость доставки вынесена в константу (`DELIVERY_COST = 300`)
- Роли разграничены через `UserSession` и фильтры `BindingSource.Filter`

## Useful Links

- [Microsoft Access](https://www.microsoft.com/ru-ru/microsoft-365/access) — СУБД
- [Windows Forms](https://learn.microsoft.com/ru-ru/dotnet/desktop/winforms/) — документация
- [ADO.NET / OleDb](https://learn.microsoft.com/ru-ru/dotnet/framework/data/adonet/ole-db-ole-db-providers) — работа с Access

## Acknowledgments & Mission

Огромная благодарность моему преподавателю за то, что заставил сделать
это по диаграммам, а не «как получится». Без этого проект был бы
набором кнопок, а не работающей системой.

И спасибо Microsoft за то, что Access до сих пор существует. Он странный,
но он работает.

## License

Этот проект распространяется под лицензией MIT.
Подробнее см. в файле [LICENSE](https://github.com/Dinosaurguk/Marketplace_MDK/blob/main/LICENSE).
