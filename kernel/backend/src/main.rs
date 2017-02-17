extern crate rustless;
extern crate iron;
extern crate engine;

mod api;
mod chess_json;

fn main() {
    let app = rustless::Application::new(api::api_root());
    iron::Iron::new(app).http("0.0.0.0:8000").unwrap();
}
