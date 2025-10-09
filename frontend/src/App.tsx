import ProductSectionDemo from './Component/ProductSection';
import HeroBannerCarousel from './Component/HeroBanner';
import AppleStyleNavbar from './Component/Header_and_navbar';
import FooterDemo from "./Component/Footer";
import ProfilePage from './pages/ProfilePage';
import ProductDetailDemo from './Component/ProductDetail';
import ShoppingCart from './Component/Cart';

function App() {
  return (
    <div className="min-h-screen mx-auto bg-gray-100">
      {/* Navbar */}
      {/* <AppleStyleNavbar /> */}
      {/* Hero Banner thường chiếm toàn bộ chiều ngang */}
      {/* <HeroBannerCarousel /> */}
      {/* Product Section */}
      {/* <ProductSectionDemo /> */}
      {/* Footer */}
      {/* <ProfilePage onNavigate={(page) => alert(`Navigate to ${page}`)} /> */}
      {/* <ProductDetailDemo/> */}
      <ShoppingCart />

      <FooterDemo />
    </div>
  )
}
export default App;
