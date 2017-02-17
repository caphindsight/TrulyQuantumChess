extern crate rustless;
extern crate iron;

mod api;

fn main() {
    let app = rustless::Application::new(api::api_root());
    iron::Iron::new(app).http("0.0.0.0:8000").unwrap();
}
