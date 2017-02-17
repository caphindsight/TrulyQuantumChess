use rustless;
use rustless::Nesting;

pub fn api_root() -> rustless::Api {
    rustless::Api::build(|api_root| {
        api_root.namespace("api", |api| {
            api.get("ping", |ping| {
                ping.handle(|client, params| {
                    client.text("pong\r\n".to_string())
                })
            });
        });
    })
}
