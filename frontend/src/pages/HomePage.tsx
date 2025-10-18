// ==================== pages/HomePage.tsx ====================
import HeaderAndNavbar from '../components/HeaderAndNavbar';
import HeroBannerCarousel from '../components/HeroBanner';
import ProductSectionDemo from '../components/ProductSection';
import Footer from '../components/Footer';



export default function HomePage() {
  return (
    <>
    <HeaderAndNavbar/>
    <HeroBannerCarousel/>
    {/* <ProductSectionDemo/> */}
    <Footer/>
    </>
  );
}
