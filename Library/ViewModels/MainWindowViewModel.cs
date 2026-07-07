using Library.Helper;
using Library.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using System.Windows.Controls;

namespace Library.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string connectionString = @"Data Source=C:\Database\Library.sqlite;";

        private string _newTitle;
        private string _newAuthor;
        private string _newGenre;
        private decimal _newPrice;
        private bool _newIsAvailable;
        private BookModel _selectedBook;
        private ObservableCollection<BookModel> _books;


        public event PropertyChangedEventHandler? PropertyChanged;

        public string NewGenre// proprietate pentru genul cărții introduse
        {
            get => _newGenre;
            set
            {
                if (_newGenre != value)
                {
                    _newGenre = value;
                    //notifica fereastra ca s-a modificat valoarea lui NewGenre
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewGenre"));
                }
            }
        }

        public string NewTitle
        {
            get => _newTitle;
            set
            {
                if (_newTitle != value)
                {
                    _newTitle = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewTitle"));
                }
            }
        }

        public string NewAuthor
        {
            get => _newAuthor;
            set
            {
                if (_newAuthor != value)
                {
                    _newAuthor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewAuthor"));
                }
            }
        }

        public decimal NewPrice
        {
            get => _newPrice;
            set
            {
                if (_newPrice != value)
                {
                    _newPrice = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewPrice"));
                }
            }
        }

        public bool NewIsAvailable
        {
            get => _newIsAvailable;
            set
            {
                if (_newIsAvailable != value)
                {
                    _newIsAvailable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewIsAvailable"));
                }
            }
        }

        public BookModel SelectedBook
        {
            get => _selectedBook;
            set
            {
                if (_selectedBook != value)
                {
                    _selectedBook = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedBook"));
                }
            }
        }

        //listă specială de obiecte care anunță automat interfața când se adaugă sau se șterge ceva
        public ObservableCollection<BookModel> Books
        {
            get => _books;
            set
            {
                if (_books != value)
                {
                    _books = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Books"));
                }
            }
        }

        public ICommand AddBookCommand { get; set; }//comanda care se execută când utilizatorul apasă un buton pentru a adăuga o carte
        public ICommand DeleteBookCommand { get; set; }

        public MainWindowViewModel()
        {
            AddBookCommand = new RelayCommand(AddBookCommandExecute);
            DeleteBookCommand = new RelayCommand(DeleteBookCommandExecute);
            LoadBooks();
        }


        public void LoadBooks()// metodă care încarcă toate cărțile din baza de date în lista Books
        {
            Books = new ObservableCollection<BookModel>();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM BOOKS ";
            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Books.Add(new BookModel
                {
                    BookId = Convert.ToInt32(reader["BOOK_ID"]),
                    Title = Convert.ToString(reader["TITLE"]),
                    Author = Convert.ToString(reader["AUTHOR"]),
                    Genre = Convert.ToString(reader["GENRE"]),
                    Price = Convert.ToDecimal(reader["PRICE"]),
                    IsAvailable = Convert.ToBoolean(reader["IS_AVAILABLE"]),

                });
            }
        }

        public void AddBook()// metodă care adaugă o nouă carte în baza de date
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string query = @"INSERT INTO BOOKS(TITLE,AUTHOR,GENRE,PRICE,IS_AVAILABLE)
                                 VALUES($title, $author, $genre, $price, $isAvailable)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("$title", NewTitle);
            command.Parameters.AddWithValue("$author", NewAuthor);
            command.Parameters.AddWithValue("$genre", NewGenre);
            command.Parameters.AddWithValue("$price", NewPrice);
            command.Parameters.AddWithValue("$isAvailable", NewIsAvailable);
            using var reader = command.ExecuteReader();
        }


        public void AddBookCommandExecute()
        {
            AddBook();
            LoadBooks();
            NewTitle = null;
            NewAuthor = null;
            NewGenre = null;
            NewPrice = 0;
            NewIsAvailable = false;

        }

        public void DeleteBook(BookModel book)
        {
            if (book == null)
                return;

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string query = @"DELETE FROM BOOKS
                     WHERE BOOK_ID = $id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("$id", book.BookId);

            command.ExecuteNonQuery();

            Books.Remove(book);
        }

        public void DeleteBookCommandExecute()
        {
            if (SelectedBook != null)
            {
                DeleteBook(SelectedBook);
                Books.Remove(SelectedBook);
            }
        }
    }
}
