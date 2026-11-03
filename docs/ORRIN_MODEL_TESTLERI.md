\# Orrin - Yapay Zeka Model Testleri



\## Gun 2 - TinyLlama Testleri



Model: tinyllama:latest



\### Test 1

Orrin karakteri icin Turkce ve tek cumlelik cevap istendi.

Sonuc: Model uzun ve anlamsiz cevaplar uretti.



\### Test 2

Orrin karakteri icin en fazla 10 kelimelik Turkce cevap istendi.

Sonuc: Model daha kisa fakat anlamsiz cevap verdi. Kelime sinirina uymadi.



\### Genel Degerlendirme

Ollama ve TinyLlama calisiyor.

Modelin Turkce karakter diyalogu performansi yetersiz.

Phi3-mini modeli bellek yetersizligi nedeniyle calistirilamadi.



\### Sonraki Adim

Orrin icin karakter talimatlari ve cevap uzunlugu sinirlandirmasi gelistirilecek.



\## Ozel Orrin Modeli Testi



Model: orrin (TinyLlama tabanli)



Orrin modeli Modelfile kullanilarak basariyla olusturuldu.

Uc farkli karakter sorusuyla test edildi.



Sonuclar:

\- Model Turkce konusma kurallarina uymadi.

\- Bazi cevaplar Ingilizce uretildi.

\- Karakter kimligi korunamadi.

\- Cevaplar anlamsiz veya eksik kaldi.



Karar:

Orrin icin hazir diyaloglar ve Ollama baglantisini birlestiren hibrit sistem gelistirilecek.

